using DataTables.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DataTables
{
    public class DataTable
    {
        public SearchResponse<T> ProcessDataTablePost<T>(IQueryable<T> results, SearchRequest request)
        {
            var searchResults = SearchResults(results, request);

            var pageResults = PageResults(searchResults, request);

            var toReturn = new SearchResponse<T>()
            {
                data = pageResults.ToList(),
                draw = request.Draw,
                recordsTotal = results.Count(),
                recordsFiltered = searchResults.Count()
            };

            return toReturn;
        }

        public SearchResponse<T> ProcessDataTablePost<T>(IEnumerable<T> results, SearchRequest request)
        {
            return ProcessDataTablePost(results.AsQueryable(), request);
        }

        //Will search all string values using contain for the results words inside the search request
        private IQueryable<T> SearchResults<T>(IQueryable<T> results, SearchRequest request)
        {
            Expression compiledEx = Expression.Constant(false);
            var parameterExp = Expression.Parameter(typeof(T));

            foreach (PropertyInfo properyInfo in typeof(T).GetProperties().Where(e => e.PropertyType == typeof(string)))
            {
                //Field Name for search
                var propertyExp = Expression.Property(parameterExp, properyInfo.Name);
                //Creates method value for string.contains()
                MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                //Adding the actual search query
                var searchQuery = Expression.Constant(request.Search.Value ?? "", typeof(string));
                //T.fieldName.contains(searchQuery)
                var containsMethodExp = Expression.Call(propertyExp, method, searchQuery);
                //Keeps adding expressions with ors
                compiledEx = Expression.OrElse(compiledEx, containsMethodExp);
            }

            //Builds the where call
            compiledEx = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { results.ElementType },
                results.Expression,
                 Expression.Lambda<Func<T, bool>>(compiledEx, parameterExp));

            //Pulls the final query to return
            var resultSet = results.Provider.CreateQuery<T>(compiledEx);

            return resultSet;
        }

        //Will page the results based on values sent in with the request object
        private IQueryable<T> PageResults<T>(IQueryable<T> results, SearchRequest request)
        {
            var skip = request.Start;
            var pageSize = request.Length;
            var orderedResults = OrderResults(results, request);
            return pageSize > 0 ? orderedResults.Skip(skip).Take(pageSize) : orderedResults;
        }

        //Will order the results based on what's passed in on the search request object
        private IQueryable<T> OrderResults<T>(IQueryable<T> results, SearchRequest request)
        {
            if (request.Order == null) return results;

            int columnIndex = request.Order[0].Column;
            string sortDirection = request.Order[0].Dir == "desc" ? "OrderByDescending" : "OrderBy";
            string columnName = request.Columns[columnIndex].Data;

            Type type = typeof(T);
            PropertyInfo property = typeof(T).GetProperty(columnName);

            ParameterExpression parameter = Expression.Parameter(type, "p");
            MemberExpression propertyAccess = Expression.MakeMemberAccess(parameter, property);

            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            MethodCallExpression resultExp = Expression.Call(
                typeof(Queryable),
                sortDirection,
                new Type[] { type, property.PropertyType },
                results.Expression,
                Expression.Quote(orderByExp));

            return results.Provider.CreateQuery<T>(resultExp);
        }
    }
}