var Connection = require('tedious').Connection;
var Request = require('tedious').Request;

var config = {

  server: 'db2tosql-svr.database.windows.net',
  authentication: {
    type: 'default',
    options: {
      userName: 'Hiccup',
      password: 'AdvKansasDCF2021'
    }
  },

  // If you're on Azure Data Warehouse, you will need this:
  options: {
    database: "master",
    MultipleActiveResultSets: true,
    Encrypt: true,
    TrustServerCertificate: false

  }
};

const connection = new Connection(config);

connection.connect((err) => {
  if (err) {
    console.log('Connection Failed');
    throw err;
  }

  executeStatement();
});

function executeStatement() {
  //CREATE DATABASE KSDCFDB_E2E AS COPY OF KSDCFDB_EBCDIC
  //DROP DATABASE KSDCFDB_E2E
  const request = new Request('CREATE DATABASE KSDCFDB_E2E AS COPY OF KSDCFDB_EBCDIC', (err, rowCount) => {
    if (err) {
      throw err;
    }

    console.log('DONE!');
    connection.close();
  });

  // Emits a 'DoneInProc' event when completed.
  request.on('row', (columns) => {
    columns.forEach((column) => {
      if (column.value === null) {
        console.log('NULL');
      } else {
        console.log(column.value);
      }
    });
  });

  request.on('done', (rowCount) => {
    console.log('Done is called!');
  });

  request.on('doneInProc', (rowCount, more) => {
    console.log(rowCount + ' rows returned');
  });

  // In SQL Server 2000 you may need: connection.execSqlBatch(request);
  connection.execSql(request);
}
