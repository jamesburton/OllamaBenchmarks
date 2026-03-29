{
  "name": "DataSource",
  "type": "interface",
  "description": "Defines a data source that can fetch batches of data asynchronously."
}
{
  "name": "DataProcessor",
  "type": "class",
  "description": "Processes data from a data source by fetching batches and yielding items.",
  "methods": [
    {
      "name": "ProcessAllBatchesAsync",
      "type": "async IAsyncEnumerable<string>",
      "description": "Processes all batches from the data source, yielding each item.",
      "parameters": [
        {
          "name": "source",
          "type": "IDataSource",
          "description": "The data source to fetch batches from."
        },
        {
          "name": "ct",
          "type": "CancellationToken",
          "description": "The cancellation token.",
          "isOptional": true,
          "defaultValue": "default"
        }
      ]
    }
  ]
}