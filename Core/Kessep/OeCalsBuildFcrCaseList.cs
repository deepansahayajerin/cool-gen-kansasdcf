// Program: OE_CALS_BUILD_FCR_CASE_LIST, ID: 374572001, model: 746.
// Short name: SWE00025
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_CALS_BUILD_FCR_CASE_LIST.
/// </summary>
[Serializable]
public partial class OeCalsBuildFcrCaseList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CALS_BUILD_FCR_CASE_LIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCalsBuildFcrCaseList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCalsBuildFcrCaseList.
  /// </summary>
  public OeCalsBuildFcrCaseList(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    // Date		Developer	Request #      Description
    // ---------------------------------------------------------------
    // 07/30/2009	M Fan		CQ7190	        Initial Dev
    // ---------------------------------------------------------------
    // ---------------------------------------------------------------
    //         ***  Read when case number entered  ***
    // ---------------------------------------------------------------
    if (!IsEmpty(import.Starting.CaseId) && Equal(global.Command, "DISPLAY"))
    {
      export.FcrCaseList.Index = -1;

      foreach(var item in ReadFcrCaseMaster1())
      {
        ++export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index >= Export.FcrCaseListGroup.Capacity)
        {
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    if (!IsEmpty(import.Starting.CaseId) && Equal(global.Command, "NEXT1"))
    {
      export.FcrCaseList.Index = -1;

      foreach(var item in ReadFcrCaseMaster2())
      {
        ++export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index >= Export.FcrCaseListGroup.Capacity)
        {
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    if (!IsEmpty(import.Starting.CaseId) && Equal(global.Command, "PREV"))
    {
      export.FcrCaseList.Index = 6;
      export.FcrCaseList.CheckSize();

      foreach(var item in ReadFcrCaseMaster7())
      {
        --export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index < 0)
        {
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    // ---------------------------------------------------------------
    //    ***  Read when date selection and date range entered ***
    // ---------------------------------------------------------------
    if (AsChar(import.Sel.SelectChar) == 'S' && Equal
      (global.Command, "DISPLAY"))
    {
      export.FcrCaseList.Index = -1;

      foreach(var item in ReadFcrCaseMaster5())
      {
        ++export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index >= Export.FcrCaseListGroup.Capacity)
        {
          export.NextPageStartingCase.CaseId = entities.FcrCaseMaster.CaseId;
          export.NextPageStartingDate.Date =
            entities.FcrCaseMaster.CaseSentDateToFcr;
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    if (AsChar(import.Sel.SelectChar) == 'R' && Equal
      (global.Command, "DISPLAY"))
    {
      export.FcrCaseList.Index = -1;

      foreach(var item in ReadFcrCaseMaster6())
      {
        ++export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index >= Export.FcrCaseListGroup.Capacity)
        {
          export.NextPageStartingCase.CaseId = entities.FcrCaseMaster.CaseId;
          export.NextPageStartingDate.Date =
            entities.FcrCaseMaster.FcrCaseResponseDate;
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    if (AsChar(import.Sel.SelectChar) == 'S' && Equal(global.Command, "NEXT1"))
    {
      export.FcrCaseList.Index = -1;

      foreach(var item in ReadFcrCaseMaster3())
      {
        ++export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index >= Export.FcrCaseListGroup.Capacity)
        {
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    if (AsChar(import.Sel.SelectChar) == 'R' && Equal(global.Command, "NEXT1"))
    {
      export.FcrCaseList.Index = -1;

      foreach(var item in ReadFcrCaseMaster4())
      {
        ++export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index >= Export.FcrCaseListGroup.Capacity)
        {
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    if (AsChar(import.Sel.SelectChar) == 'S' && Equal(global.Command, "PREV"))
    {
      export.FcrCaseList.Index = 6;
      export.FcrCaseList.CheckSize();

      foreach(var item in ReadFcrCaseMaster8())
      {
        --export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index < 0)
        {
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }

      return;
    }

    if (AsChar(import.Sel.SelectChar) == 'R' && Equal(global.Command, "PREV"))
    {
      export.FcrCaseList.Index = 6;
      export.FcrCaseList.CheckSize();

      foreach(var item in ReadFcrCaseMaster9())
      {
        --export.FcrCaseList.Index;
        export.FcrCaseList.CheckSize();

        if (export.FcrCaseList.Index < 0)
        {
          export.MoreThan1Table.SelectChar = "Y";

          break;
        }

        export.FcrCaseList.Update.SelectChar.SelectChar = "";
        export.FcrCaseList.Update.FcrCaseInfo.Assign(entities.FcrCaseMaster);
      }
    }
  }

  private IEnumerable<bool> ReadFcrCaseMaster1()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster1",
      (db, command) =>
      {
        db.SetString(command, "caseId", import.Starting.CaseId);
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster2()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster2",
      (db, command) =>
      {
        db.SetString(command, "caseId", import.PrevPageEndingCase.CaseId);
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster3()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster3",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.To.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "applSentDt",
          import.PrevPageEndingDate.Date.GetValueOrDefault());
        db.SetString(command, "caseId", import.PrevPageEndingCase.CaseId);
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster4()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster4",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.To.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fcrResponseDt",
          import.PrevPageEndingDate.Date.GetValueOrDefault());
        db.SetString(command, "caseId", import.PrevPageEndingCase.CaseId);
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster5()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster5",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.To.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster6()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster6",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.To.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster7()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster7",
      (db, command) =>
      {
        db.SetString(command, "caseId", import.PrevPageEndingCase.CaseId);
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster8()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster8",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.To.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "applSentDt",
          import.PrevPageEndingDate.Date.GetValueOrDefault());
        db.SetString(command, "caseId", import.PrevPageEndingCase.CaseId);
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFcrCaseMaster9()
  {
    entities.FcrCaseMaster.Populated = false;

    return ReadEach("ReadFcrCaseMaster9",
      (db, command) =>
      {
        db.SetDate(command, "date1", import.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", import.To.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "fcrResponseDt",
          import.PrevPageEndingDate.Date.GetValueOrDefault());
        db.SetString(command, "caseId", import.PrevPageEndingCase.CaseId);
      },
      (db, reader) =>
      {
        entities.FcrCaseMaster.CaseId = db.GetString(reader, 0);
        entities.FcrCaseMaster.OrderIndicator = db.GetNullableString(reader, 1);
        entities.FcrCaseMaster.ActionTypeCode = db.GetNullableString(reader, 2);
        entities.FcrCaseMaster.BatchNumber = db.GetNullableString(reader, 3);
        entities.FcrCaseMaster.FipsCountyCode = db.GetNullableString(reader, 4);
        entities.FcrCaseMaster.PreviousCaseId = db.GetNullableString(reader, 5);
        entities.FcrCaseMaster.CaseSentDateToFcr =
          db.GetNullableDate(reader, 6);
        entities.FcrCaseMaster.FcrCaseResponseDate =
          db.GetNullableDate(reader, 7);
        entities.FcrCaseMaster.AcknowlegementCode =
          db.GetNullableString(reader, 8);
        entities.FcrCaseMaster.ErrorCode1 = db.GetNullableString(reader, 9);
        entities.FcrCaseMaster.ErrorCode2 = db.GetNullableString(reader, 10);
        entities.FcrCaseMaster.ErrorCode3 = db.GetNullableString(reader, 11);
        entities.FcrCaseMaster.ErrorCode4 = db.GetNullableString(reader, 12);
        entities.FcrCaseMaster.ErrorCode5 = db.GetNullableString(reader, 13);
        entities.FcrCaseMaster.CreatedBy = db.GetString(reader, 14);
        entities.FcrCaseMaster.CreatedTimestamp = db.GetDateTime(reader, 15);
        entities.FcrCaseMaster.RecordIdentifier =
          db.GetNullableString(reader, 16);
        entities.FcrCaseMaster.FcrCaseComments =
          db.GetNullableString(reader, 17);
        entities.FcrCaseMaster.Populated = true;

        return true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of PrevPageEndingDate.
    /// </summary>
    [JsonPropertyName("prevPageEndingDate")]
    public DateWorkArea PrevPageEndingDate
    {
      get => prevPageEndingDate ??= new();
      set => prevPageEndingDate = value;
    }

    /// <summary>
    /// A value of PrevPageEndingCase.
    /// </summary>
    [JsonPropertyName("prevPageEndingCase")]
    public FcrCaseMaster PrevPageEndingCase
    {
      get => prevPageEndingCase ??= new();
      set => prevPageEndingCase = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public FcrCaseMaster Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Sel.
    /// </summary>
    [JsonPropertyName("sel")]
    public Common Sel
    {
      get => sel ??= new();
      set => sel = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    private DateWorkArea prevPageEndingDate;
    private FcrCaseMaster prevPageEndingCase;
    private FcrCaseMaster starting;
    private Common sel;
    private DateWorkArea from;
    private DateWorkArea to;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FcrCaseListGroup group.</summary>
    [Serializable]
    public class FcrCaseListGroup
    {
      /// <summary>
      /// A value of SelectChar.
      /// </summary>
      [JsonPropertyName("selectChar")]
      public Common SelectChar
      {
        get => selectChar ??= new();
        set => selectChar = value;
      }

      /// <summary>
      /// A value of FcrCaseInfo.
      /// </summary>
      [JsonPropertyName("fcrCaseInfo")]
      public FcrCaseMaster FcrCaseInfo
      {
        get => fcrCaseInfo ??= new();
        set => fcrCaseInfo = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common selectChar;
      private FcrCaseMaster fcrCaseInfo;
    }

    /// <summary>
    /// A value of PrevPageEndDate.
    /// </summary>
    [JsonPropertyName("prevPageEndDate")]
    public DateWorkArea PrevPageEndDate
    {
      get => prevPageEndDate ??= new();
      set => prevPageEndDate = value;
    }

    /// <summary>
    /// A value of PrevPageEndCase.
    /// </summary>
    [JsonPropertyName("prevPageEndCase")]
    public FcrCaseMaster PrevPageEndCase
    {
      get => prevPageEndCase ??= new();
      set => prevPageEndCase = value;
    }

    /// <summary>
    /// A value of MoreThan1Table.
    /// </summary>
    [JsonPropertyName("moreThan1Table")]
    public Common MoreThan1Table
    {
      get => moreThan1Table ??= new();
      set => moreThan1Table = value;
    }

    /// <summary>
    /// A value of NextPageStartingCase.
    /// </summary>
    [JsonPropertyName("nextPageStartingCase")]
    public FcrCaseMaster NextPageStartingCase
    {
      get => nextPageStartingCase ??= new();
      set => nextPageStartingCase = value;
    }

    /// <summary>
    /// A value of NextPageStartingDate.
    /// </summary>
    [JsonPropertyName("nextPageStartingDate")]
    public DateWorkArea NextPageStartingDate
    {
      get => nextPageStartingDate ??= new();
      set => nextPageStartingDate = value;
    }

    /// <summary>
    /// Gets a value of FcrCaseList.
    /// </summary>
    [JsonIgnore]
    public Array<FcrCaseListGroup> FcrCaseList => fcrCaseList ??= new(
      FcrCaseListGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FcrCaseList for json serialization.
    /// </summary>
    [JsonPropertyName("fcrCaseList")]
    [Computed]
    public IList<FcrCaseListGroup> FcrCaseList_Json
    {
      get => fcrCaseList;
      set => FcrCaseList.Assign(value);
    }

    private DateWorkArea prevPageEndDate;
    private FcrCaseMaster prevPageEndCase;
    private Common moreThan1Table;
    private FcrCaseMaster nextPageStartingCase;
    private DateWorkArea nextPageStartingDate;
    private Array<FcrCaseListGroup> fcrCaseList;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FcrCaseMaster.
    /// </summary>
    [JsonPropertyName("fcrCaseMaster")]
    public FcrCaseMaster FcrCaseMaster
    {
      get => fcrCaseMaster ??= new();
      set => fcrCaseMaster = value;
    }

    private FcrCaseMaster fcrCaseMaster;
  }
#endregion
}
