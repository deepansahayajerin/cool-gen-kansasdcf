import { Connection, Request } from "tedious";

export class DbUtils
{
  config = {

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

  connection = new Connection(this.config);

  async resetDB(DBName: string, CopyOf: string)
  {
    await this.dropDB(DBName);
    //await this.createDB(DBName, CopyOf);
  }

  async connectDB()
  {
    await this.connection.connect((err) =>
    {
      if (err)
      {
        console.log('Connection Failed');
        throw err;
      }
    });
  }

  async dropDB(DBName: string)
  {
    console.log("in dropDB");
    await this.connectDB();
    const request = new Request(`DROP DATABASE ${ DBName }`, async (err) =>
    {
      if (err)
      {
        throw err;
      }

    });
    console.log("in dropDB before execSQL");
    await this.connection.execSql(request);
    console.log("in dropDB after execSQL");
    await request.on('requestCompleted', function () 
    {
      console.log(`DB ${ DBName } dropped`);
    });
    console.log("in dropDB before connection close");
    await this.connection.close();
    console.log("in dropDB after connection close");
  }

  async createDB(DBName: string, CopyOf: string)
  {
    console.log("in createDB");
    await this.connectDB();
    console.log("in createDB after connect");
    const request = new Request(`CREATE DATABASE ${ DBName } AS COPY OF ${ CopyOf }`, async (err) =>
    {
      if (err)
      {
        throw err;
      }
    });
    console.log("in createDB before execSql");
    await this.connection.execSql(request);
    console.log("in createDB after execSql");
    await request.on('requestCompleted', function () 
    {
      console.log(`DB ${ DBName } created as copy of ${ CopyOf }`);
    });
    console.log("in createDB before connection close");
    await this.connection.close();
    console.log("in createDB after connection close");
  }

}