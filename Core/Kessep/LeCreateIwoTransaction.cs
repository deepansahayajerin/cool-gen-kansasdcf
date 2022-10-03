// Program: LE_CREATE_IWO_TRANSACTION, ID: 1902468730, model: 746.
// Short name: SWE00838
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_CREATE_IWO_TRANSACTION.
/// </summary>
[Serializable]
public partial class LeCreateIwoTransaction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CREATE_IWO_TRANSACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCreateIwoTransaction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCreateIwoTransaction.
  /// </summary>
  public LeCreateIwoTransaction(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ---------------------------------------------
    // 06/09/15  GVandy	CQ22212		Initial Code
    // -------------------------------------------------------------------------------------
    export.IwoTransaction.TransactionNumber =
      import.IwoTransaction.TransactionNumber;
    MoveIwoAction(import.IwoAction, export.IwoAction);

    // -- Establish currency on the legal action
    if (!ReadLegalAction())
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    // -- Establish currency on the cse person
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // -- If the infrastructure identifier was provided then establish currency 
    // on infrastructure
    if (import.Infrastructure.SystemGeneratedIdentifier > 0)
    {
      if (!ReadInfrastructure())
      {
        ExitState = "INFRASTRUCTURE_NF";

        return;
      }

      if (!ReadOutgoingDocument())
      {
        ExitState = "OUTGOING_DOCUMENT_NF";

        return;
      }
    }

    if (!IsEmpty(import.OutgoingDocument.PrintSucessfulIndicator) && !
      entities.OutgoingDocument.Populated)
    {
      ExitState = "OUTGOING_DOCUMENT_NF";

      return;
    }

    // -- If the income source identifier was provided then establish currency 
    // on income source
    if (Lt(local.Null1.Timestamp, import.IncomeSource.Identifier))
    {
      if (!ReadIncomeSource())
      {
        ExitState = "INCOME_SOURCE_NF";

        return;
      }
    }

    if (IsEmpty(import.IwoTransaction.TransactionNumber))
    {
      // -- Determine the largest IWO Transaction identifier for this cse person
      // and legal action.
      ReadIwoTransaction2();
      ++local.MaxIwoTransaction.Identifier;

      // -- Determine the largest IWO Transaction Transaction Number.
      if (Equal(import.IwoAction.ActionType, "PRINT"))
      {
        local.MaxIwoTransaction.TransactionNumber = "";
      }
      else
      {
        ReadIwoTransaction3();
        local.MaxIwoTransaction.TransactionNumber =
          NumberToString(StringToNumber(
            local.MaxIwoTransaction.TransactionNumber) + 1, 4, 12);
      }

      // -- Create a new IWO Transaction.
      try
      {
        CreateIwoTransaction();
        export.IwoTransaction.Assign(entities.IwoTransaction);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "IWO_TRANSACTION_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "IWO_TRANSACTION_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (entities.IncomeSource.Populated)
      {
        AssociateIwoTransaction();
      }
    }
    else
    {
      // -- Establish currency on IWO Transaction
      if (!ReadIwoTransaction1())
      {
        ExitState = "IWO_TRANSACTION_NF";

        return;
      }

      // -- Update the IWO Transaction status.
      try
      {
        UpdateIwoTransaction();
        export.IwoTransaction.Assign(entities.IwoTransaction);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "IWO_TRANSACTION_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "IWO_TRANSACTION_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // -- Determine the largest IWO Action identifier for this IWO Transaction.
    ReadIwoAction2();

    if (Equal(import.IwoAction.ActionType, "RESUB"))
    {
      // -- Set the severity cleared indicator on the current IWO action.
      if (ReadIwoAction1())
      {
        try
        {
          UpdateIwoAction();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "IWO_ACTION_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "IWO_ACTION_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "IWO_ACTION_NF";
      }
    }

    ++local.MaxIwoAction.Identifier;

    // -- Create a new IWO Action.
    try
    {
      CreateIwoAction();
      export.IwoAction.Assign(entities.IwoAction);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "IWO_ACTION_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "IWO_ACTION_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (entities.OutgoingDocument.Populated)
    {
      AssociateIwoAction();
    }

    // -- Create a new IWO Action History.
    try
    {
      CreateIwoActionHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "IWO_ACTION_HISTORY_AE";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "IWO_ACTION_HISTORY_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!IsEmpty(import.OutgoingDocument.PrintSucessfulIndicator))
    {
      // -- Update the Outgoing Document print successful indicator.
      try
      {
        UpdateOutgoingDocument();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "OUTGOING_DOCUMENT_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "OUTGOING_DOCUMENT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      // -- Update the Infrastructure Detail message.
      try
      {
        UpdateInfrastructure();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SP0000_INFRASTRUCTURE_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SP0000_INFRASTRUCTURE_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveIwoAction(IwoAction source, IwoAction target)
  {
    target.ActionType = source.ActionType;
    target.StatusCd = source.StatusCd;
  }

  private void AssociateIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var infId = entities.OutgoingDocument.InfId;

    entities.IwoAction.Populated = false;
    Update("AssociateIwoAction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "infId", infId);
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.InfId = infId;
    entities.IwoAction.Populated = true;
  }

  private void AssociateIwoTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var cspINumber = entities.IncomeSource.CspINumber;
    var isrIdentifier = entities.IncomeSource.Identifier;

    entities.IwoTransaction.Populated = false;
    Update("AssociateIwoTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "cspINumber", cspINumber);
        db.SetNullableDateTime(command, "isrIdentifier", isrIdentifier);
        db.SetInt32(command, "identifier", entities.IwoTransaction.Identifier);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
      });

    entities.IwoTransaction.CspINumber = cspINumber;
    entities.IwoTransaction.IsrIdentifier = isrIdentifier;
    entities.IwoTransaction.Populated = true;
  }

  private void CreateIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);

    var identifier = local.MaxIwoAction.Identifier;
    var actionType = import.IwoAction.ActionType ?? "";
    var statusCd = import.IwoAction.StatusCd ?? "";
    var statusDate = Now().Date;
    var severityClearedInd = "N";
    var errorField1 = Spaces(32);
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspNumber = entities.IwoTransaction.CspNumber;
    var lgaIdentifier = entities.IwoTransaction.LgaIdentifier;
    var iwtIdentifier = entities.IwoTransaction.Identifier;

    entities.IwoAction.Populated = false;
    Update("CreateIwoAction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "actionType", actionType);
        db.SetNullableString(command, "statusCd", statusCd);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "statusReasonCd", "");
        db.SetNullableString(command, "docTrackingId", "");
        db.SetNullableString(command, "fileControlId", "");
        db.SetNullableString(command, "batchControlId", "");
        db.SetNullableString(command, "svrityClearedInd", severityClearedInd);
        db.SetNullableString(command, "errorRecordType", "");
        db.SetNullableString(command, "errorField1", errorField1);
        db.SetNullableString(command, "errorField2", errorField1);
        db.SetNullableString(command, "moreErrorsInd", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", iwtIdentifier);
      });

    entities.IwoAction.Identifier = identifier;
    entities.IwoAction.ActionType = actionType;
    entities.IwoAction.StatusCd = statusCd;
    entities.IwoAction.StatusDate = statusDate;
    entities.IwoAction.StatusReasonCode = "";
    entities.IwoAction.DocumentTrackingIdentifier = "";
    entities.IwoAction.FileControlId = "";
    entities.IwoAction.BatchControlId = "";
    entities.IwoAction.SeverityClearedInd = severityClearedInd;
    entities.IwoAction.ErrorRecordType = "";
    entities.IwoAction.ErrorField1 = errorField1;
    entities.IwoAction.ErrorField2 = errorField1;
    entities.IwoAction.MoreErrorsInd = "";
    entities.IwoAction.CreatedBy = createdBy;
    entities.IwoAction.CreatedTimestamp = createdTimestamp;
    entities.IwoAction.LastUpdatedBy = createdBy;
    entities.IwoAction.LastUpdatedTimestamp = createdTimestamp;
    entities.IwoAction.CspNumber = cspNumber;
    entities.IwoAction.LgaIdentifier = lgaIdentifier;
    entities.IwoAction.IwtIdentifier = iwtIdentifier;
    entities.IwoAction.InfId = null;
    entities.IwoAction.Populated = true;
  }

  private void CreateIwoActionHistory()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var identifier = 1;
    var actionTaken = import.IwoAction.StatusCd ?? "";
    var actionDate = Now().Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspNumber = entities.IwoAction.CspNumber;
    var lgaIdentifier = entities.IwoAction.LgaIdentifier;
    var iwtIdentifier = entities.IwoAction.IwtIdentifier;
    var iwaIdentifier = entities.IwoAction.Identifier;

    entities.IwoActionHistory.Populated = false;
    Update("CreateIwoActionHistory",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "actionTaken", actionTaken);
        db.SetNullableDate(command, "actionDate", actionDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", iwtIdentifier);
        db.SetInt32(command, "iwaIdentifier", iwaIdentifier);
      });

    entities.IwoActionHistory.Identifier = identifier;
    entities.IwoActionHistory.ActionTaken = actionTaken;
    entities.IwoActionHistory.ActionDate = actionDate;
    entities.IwoActionHistory.CreatedBy = createdBy;
    entities.IwoActionHistory.CreatedTimestamp = createdTimestamp;
    entities.IwoActionHistory.CspNumber = cspNumber;
    entities.IwoActionHistory.LgaIdentifier = lgaIdentifier;
    entities.IwoActionHistory.IwtIdentifier = iwtIdentifier;
    entities.IwoActionHistory.IwaIdentifier = iwaIdentifier;
    entities.IwoActionHistory.Populated = true;
  }

  private void CreateIwoTransaction()
  {
    var identifier = local.MaxIwoTransaction.Identifier;
    var transactionNumber = local.MaxIwoTransaction.TransactionNumber ?? "";
    var currentStatus = import.IwoAction.StatusCd ?? "";
    var statusDate = Now().Date;
    var note = Spaces(80);
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lgaIdentifier = entities.LegalAction.Identifier;
    var cspNumber = entities.CsePerson.Number;

    entities.IwoTransaction.Populated = false;
    Update("CreateIwoTransaction",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "transactionNumber", transactionNumber);
        db.SetNullableString(command, "currentStatus", currentStatus);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "note", note);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetInt32(command, "lgaIdentifier", lgaIdentifier);
        db.SetString(command, "cspNumber", cspNumber);
      });

    entities.IwoTransaction.Identifier = identifier;
    entities.IwoTransaction.TransactionNumber = transactionNumber;
    entities.IwoTransaction.CurrentStatus = currentStatus;
    entities.IwoTransaction.StatusDate = statusDate;
    entities.IwoTransaction.Note = note;
    entities.IwoTransaction.CreatedBy = createdBy;
    entities.IwoTransaction.CreatedTimestamp = createdTimestamp;
    entities.IwoTransaction.LastUpdatedBy = "";
    entities.IwoTransaction.LastUpdatedTimestamp = null;
    entities.IwoTransaction.LgaIdentifier = lgaIdentifier;
    entities.IwoTransaction.CspNumber = cspNumber;
    entities.IwoTransaction.CspINumber = null;
    entities.IwoTransaction.IsrIdentifier = null;
    entities.IwoTransaction.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.Populated = true;
      });
  }

  private bool ReadInfrastructure()
  {
    entities.Infrastructure.Populated = false;

    return Read("ReadInfrastructure",
      (db, command) =>
      {
        db.SetInt32(
          command, "systemGeneratedI",
          import.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Infrastructure.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Infrastructure.Detail = db.GetNullableString(reader, 1);
        entities.Infrastructure.Populated = true;
      });
  }

  private bool ReadIwoAction1()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    entities.IwoAction.Populated = false;

    return Read("ReadIwoAction1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", local.MaxIwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
      },
      (db, reader) =>
      {
        entities.IwoAction.Identifier = db.GetInt32(reader, 0);
        entities.IwoAction.ActionType = db.GetNullableString(reader, 1);
        entities.IwoAction.StatusCd = db.GetNullableString(reader, 2);
        entities.IwoAction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoAction.StatusReasonCode = db.GetNullableString(reader, 4);
        entities.IwoAction.DocumentTrackingIdentifier =
          db.GetNullableString(reader, 5);
        entities.IwoAction.FileControlId = db.GetNullableString(reader, 6);
        entities.IwoAction.BatchControlId = db.GetNullableString(reader, 7);
        entities.IwoAction.SeverityClearedInd = db.GetNullableString(reader, 8);
        entities.IwoAction.ErrorRecordType = db.GetNullableString(reader, 9);
        entities.IwoAction.ErrorField1 = db.GetNullableString(reader, 10);
        entities.IwoAction.ErrorField2 = db.GetNullableString(reader, 11);
        entities.IwoAction.MoreErrorsInd = db.GetNullableString(reader, 12);
        entities.IwoAction.CreatedBy = db.GetString(reader, 13);
        entities.IwoAction.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.IwoAction.LastUpdatedBy = db.GetNullableString(reader, 15);
        entities.IwoAction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.IwoAction.CspNumber = db.GetString(reader, 17);
        entities.IwoAction.LgaIdentifier = db.GetInt32(reader, 18);
        entities.IwoAction.IwtIdentifier = db.GetInt32(reader, 19);
        entities.IwoAction.InfId = db.GetNullableInt32(reader, 20);
        entities.IwoAction.Populated = true;
      });
  }

  private bool ReadIwoAction2()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);
    local.MaxIwoAction.Populated = false;

    return Read("ReadIwoAction2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.
          SetInt32(command, "iwtIdentifier", entities.IwoTransaction.Identifier);
          
      },
      (db, reader) =>
      {
        local.MaxIwoAction.Identifier = db.GetInt32(reader, 0);
        local.MaxIwoAction.Populated = true;
      });
  }

  private bool ReadIwoTransaction1()
  {
    entities.IwoTransaction.Populated = false;

    return Read("ReadIwoTransaction1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "transactionNumber",
          import.IwoTransaction.TransactionNumber ?? "");
      },
      (db, reader) =>
      {
        entities.IwoTransaction.Identifier = db.GetInt32(reader, 0);
        entities.IwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 1);
        entities.IwoTransaction.CurrentStatus = db.GetNullableString(reader, 2);
        entities.IwoTransaction.StatusDate = db.GetNullableDate(reader, 3);
        entities.IwoTransaction.Note = db.GetNullableString(reader, 4);
        entities.IwoTransaction.CreatedBy = db.GetString(reader, 5);
        entities.IwoTransaction.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.IwoTransaction.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.IwoTransaction.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 8);
        entities.IwoTransaction.LgaIdentifier = db.GetInt32(reader, 9);
        entities.IwoTransaction.CspNumber = db.GetString(reader, 10);
        entities.IwoTransaction.CspINumber = db.GetNullableString(reader, 11);
        entities.IwoTransaction.IsrIdentifier =
          db.GetNullableDateTime(reader, 12);
        entities.IwoTransaction.Populated = true;
      });
  }

  private bool ReadIwoTransaction2()
  {
    local.MaxIwoTransaction.Populated = false;

    return Read("ReadIwoTransaction2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        local.MaxIwoTransaction.Identifier = db.GetInt32(reader, 0);
        local.MaxIwoTransaction.Populated = true;
      });
  }

  private bool ReadIwoTransaction3()
  {
    local.MaxIwoTransaction.Populated = false;

    return Read("ReadIwoTransaction3",
      null,
      (db, reader) =>
      {
        local.MaxIwoTransaction.TransactionNumber =
          db.GetNullableString(reader, 0);
        local.MaxIwoTransaction.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadOutgoingDocument()
  {
    entities.OutgoingDocument.Populated = false;

    return Read("ReadOutgoingDocument",
      (db, command) =>
      {
        db.SetInt32(
          command, "infId", entities.Infrastructure.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.OutgoingDocument.PrintSucessfulIndicator =
          db.GetString(reader, 0);
        entities.OutgoingDocument.LastUpdatedBy =
          db.GetNullableString(reader, 1);
        entities.OutgoingDocument.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 2);
        entities.OutgoingDocument.InfId = db.GetInt32(reader, 3);
        entities.OutgoingDocument.Populated = true;
      });
  }

  private void UpdateInfrastructure()
  {
    var detail = "E-IWO submitted for processing.";

    entities.Infrastructure.Populated = false;
    Update("UpdateInfrastructure",
      (db, command) =>
      {
        db.SetNullableString(command, "detail", detail);
        db.SetInt32(
          command, "systemGeneratedI",
          entities.Infrastructure.SystemGeneratedIdentifier);
      });

    entities.Infrastructure.Detail = detail;
    entities.Infrastructure.Populated = true;
  }

  private void UpdateIwoAction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoAction.Populated);

    var severityClearedInd = "Y";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.IwoAction.Populated = false;
    Update("UpdateIwoAction",
      (db, command) =>
      {
        db.SetNullableString(command, "svrityClearedInd", severityClearedInd);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", entities.IwoAction.Identifier);
        db.SetString(command, "cspNumber", entities.IwoAction.CspNumber);
        db.SetInt32(command, "lgaIdentifier", entities.IwoAction.LgaIdentifier);
        db.SetInt32(command, "iwtIdentifier", entities.IwoAction.IwtIdentifier);
      });

    entities.IwoAction.SeverityClearedInd = severityClearedInd;
    entities.IwoAction.LastUpdatedBy = lastUpdatedBy;
    entities.IwoAction.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IwoAction.Populated = true;
  }

  private void UpdateIwoTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.IwoTransaction.Populated);

    var currentStatus = import.IwoAction.StatusCd ?? "";
    var statusDate = Now().Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.IwoTransaction.Populated = false;
    Update("UpdateIwoTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "currentStatus", currentStatus);
        db.SetNullableDate(command, "statusDate", statusDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "identifier", entities.IwoTransaction.Identifier);
        db.SetInt32(
          command, "lgaIdentifier", entities.IwoTransaction.LgaIdentifier);
        db.SetString(command, "cspNumber", entities.IwoTransaction.CspNumber);
      });

    entities.IwoTransaction.CurrentStatus = currentStatus;
    entities.IwoTransaction.StatusDate = statusDate;
    entities.IwoTransaction.LastUpdatedBy = lastUpdatedBy;
    entities.IwoTransaction.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IwoTransaction.Populated = true;
  }

  private void UpdateOutgoingDocument()
  {
    System.Diagnostics.Debug.Assert(entities.OutgoingDocument.Populated);

    var printSucessfulIndicator =
      import.OutgoingDocument.PrintSucessfulIndicator;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();

    entities.OutgoingDocument.Populated = false;
    Update("UpdateOutgoingDocument",
      (db, command) =>
      {
        db.SetString(command, "prntSucessfulInd", printSucessfulIndicator);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetInt32(command, "infId", entities.OutgoingDocument.InfId);
      });

    entities.OutgoingDocument.PrintSucessfulIndicator = printSucessfulIndicator;
    entities.OutgoingDocument.LastUpdatedBy = lastUpdatedBy;
    entities.OutgoingDocument.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.OutgoingDocument.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    private OutgoingDocument outgoingDocument;
    private Infrastructure infrastructure;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private LegalAction legalAction;
    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaxIwoAction.
    /// </summary>
    [JsonPropertyName("maxIwoAction")]
    public IwoAction MaxIwoAction
    {
      get => maxIwoAction ??= new();
      set => maxIwoAction = value;
    }

    /// <summary>
    /// A value of MaxIwoTransaction.
    /// </summary>
    [JsonPropertyName("maxIwoTransaction")]
    public IwoTransaction MaxIwoTransaction
    {
      get => maxIwoTransaction ??= new();
      set => maxIwoTransaction = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private IwoAction maxIwoAction;
    private IwoTransaction maxIwoTransaction;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IwoActionHistory.
    /// </summary>
    [JsonPropertyName("iwoActionHistory")]
    public IwoActionHistory IwoActionHistory
    {
      get => iwoActionHistory ??= new();
      set => iwoActionHistory = value;
    }

    /// <summary>
    /// A value of OutgoingDocument.
    /// </summary>
    [JsonPropertyName("outgoingDocument")]
    public OutgoingDocument OutgoingDocument
    {
      get => outgoingDocument ??= new();
      set => outgoingDocument = value;
    }

    /// <summary>
    /// A value of IwoAction.
    /// </summary>
    [JsonPropertyName("iwoAction")]
    public IwoAction IwoAction
    {
      get => iwoAction ??= new();
      set => iwoAction = value;
    }

    /// <summary>
    /// A value of IwoTransaction.
    /// </summary>
    [JsonPropertyName("iwoTransaction")]
    public IwoTransaction IwoTransaction
    {
      get => iwoTransaction ??= new();
      set => iwoTransaction = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private IwoActionHistory iwoActionHistory;
    private OutgoingDocument outgoingDocument;
    private IwoAction iwoAction;
    private IwoTransaction iwoTransaction;
    private IncomeSource incomeSource;
    private CsePerson csePerson;
    private Infrastructure infrastructure;
    private LegalAction legalAction;
  }
#endregion
}
