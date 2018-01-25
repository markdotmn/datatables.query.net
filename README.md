# datatables.query.net
This is a quick and simple package that handles requests and output for DataTables.net.

Example usage below:

```cs
public IHttpActionResult Post(SearchRequest request)
{
  //Where Data is a flattened object(single level only) that you want to use in a datatable.
	var data = service.GetData();
	DataTable dt = new DataTable();
	var output = dt.ProcessDataTablePost(data,request);
	return Ok(output);
}
```
