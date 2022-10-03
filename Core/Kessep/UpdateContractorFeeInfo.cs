// Program: UPDATE_CONTRACTOR_FEE_INFO, ID: 371803703, model: 746.
// Short name: SWE01511
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: UPDATE_CONTRACTOR_FEE_INFO.
/// </summary>
[Serializable]
public partial class UpdateContractorFeeInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the UPDATE_CONTRACTOR_FEE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new UpdateContractorFeeInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of UpdateContractorFeeInfo.
  /// </summary>
  public UpdateContractorFeeInfo(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************
    // Action block will use the information passed into it to UPDATE
    // a CONTRACTOR FEE INFORMATION record.
    // ************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Update.Flag = "";
    MoveContractorFeeInformation(import.ContractorFeeInformation,
      export.ContractorFeeInformation);

    if (ReadOffice())
    {
      if (!IsEmpty(import.ContractorFeeInformation.JudicialDistrict) && IsEmpty
        (import.ContractorFeeInformation.DistributionProgramType) && IsEmpty
        (import.ObligationType.Code))
      {
        ReadContractorFeeInformation3();
        ReadContractorFeeInformation5();
      }
      else if (!IsEmpty(import.ContractorFeeInformation.JudicialDistrict) && !
        IsEmpty(import.ContractorFeeInformation.DistributionProgramType) && IsEmpty
        (import.ObligationType.Code))
      {
        ReadContractorFeeInformation2();
        ReadContractorFeeInformation4();
      }
      else if (!IsEmpty(import.ContractorFeeInformation.JudicialDistrict) && !
        IsEmpty(import.ContractorFeeInformation.DistributionProgramType) && !
        IsEmpty(import.ObligationType.Code))
      {
        if (ReadContractorFeeInformation2())
        {
          ReadObligationType1();
        }

        if (ReadContractorFeeInformation4())
        {
          ReadObligationType2();
        }
      }
      else if (!IsEmpty(import.ContractorFeeInformation.JudicialDistrict) && !
        IsEmpty(import.ObligationType.Code) && IsEmpty
        (import.ContractorFeeInformation.DistributionProgramType))
      {
        if (ReadContractorFeeInformation3())
        {
          ReadObligationType1();
        }

        if (ReadContractorFeeInformation5())
        {
          ReadObligationType2();
        }
      }

      if (!Lt(entities.Test1ContractorFeeInformation.DiscontinueDate,
        import.ContractorFeeInformation.DiscontinueDate) || !
        Lt(import.ContractorFeeInformation.EffectiveDate,
        entities.Test2ContractorFeeInformation.EffectiveDate))
      {
        local.Update.Flag = "Y";
      }
      else if (entities.Test1ContractorFeeInformation.
        SystemGeneratedIdentifier != entities
        .Test2ContractorFeeInformation.SystemGeneratedIdentifier && (
          !Lt(entities.Test1ContractorFeeInformation.EffectiveDate,
        import.ContractorFeeInformation.EffectiveDate) && !
        Lt(import.ContractorFeeInformation.DiscontinueDate,
        entities.Test2ContractorFeeInformation.DiscontinueDate) || !
        Lt(entities.Test1ContractorFeeInformation.EffectiveDate,
        import.ContractorFeeInformation.EffectiveDate) && import
        .ContractorFeeInformation.SystemGeneratedIdentifier != entities
        .Test1ContractorFeeInformation.SystemGeneratedIdentifier || !
        Lt(import.ContractorFeeInformation.DiscontinueDate,
        entities.Test2ContractorFeeInformation.DiscontinueDate) && import
        .ContractorFeeInformation.SystemGeneratedIdentifier != entities
        .Test2ContractorFeeInformation.SystemGeneratedIdentifier))
      {
        ExitState = "ACO_NE0000_DATE_OVERLAP";

        return;
      }
      else
      {
        if (!IsEmpty(import.ObligationType.Code) && !
          IsEmpty(import.ContractorFeeInformation.DistributionProgramType))
        {
          foreach(var item in ReadContractorFeeInformationObligationType1())
          {
            if (Lt(entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.DiscontinueDate) && Lt
              (import.ContractorFeeInformation.DiscontinueDate,
              entities.ContractorFeeInformation.DiscontinueDate) || Lt
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.EffectiveDate) && Lt
              (import.ContractorFeeInformation.EffectiveDate,
              entities.ContractorFeeInformation.DiscontinueDate) || Equal
              (entities.ContractorFeeInformation.DiscontinueDate,
              import.ContractorFeeInformation.EffectiveDate) || Equal
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.DiscontinueDate) || Equal
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.EffectiveDate) || Equal
              (entities.ContractorFeeInformation.DiscontinueDate,
              import.ContractorFeeInformation.DiscontinueDate))
            {
              ExitState = "ACO_NE0000_DATE_OVERLAP";

              return;
            }
            else
            {
              continue;
            }
          }
        }
        else if (!IsEmpty(import.ObligationType.Code) && IsEmpty
          (import.ContractorFeeInformation.DistributionProgramType))
        {
          foreach(var item in ReadContractorFeeInformationObligationType2())
          {
            if (Lt(entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.DiscontinueDate) && Lt
              (import.ContractorFeeInformation.DiscontinueDate,
              entities.ContractorFeeInformation.DiscontinueDate) || Lt
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.EffectiveDate) && Lt
              (import.ContractorFeeInformation.EffectiveDate,
              entities.ContractorFeeInformation.DiscontinueDate) || Equal
              (entities.ContractorFeeInformation.DiscontinueDate,
              import.ContractorFeeInformation.EffectiveDate) || Equal
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.DiscontinueDate) || Equal
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.EffectiveDate) || Equal
              (entities.ContractorFeeInformation.DiscontinueDate,
              import.ContractorFeeInformation.DiscontinueDate))
            {
              ExitState = "ACO_NE0000_DATE_OVERLAP";

              return;
            }
            else
            {
              continue;
            }
          }
        }
        else if (IsEmpty(import.ObligationType.Code) && !
          IsEmpty(import.ContractorFeeInformation.DistributionProgramType))
        {
          foreach(var item in ReadContractorFeeInformation6())
          {
            if (Lt(entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.DiscontinueDate) && Lt
              (import.ContractorFeeInformation.DiscontinueDate,
              entities.ContractorFeeInformation.DiscontinueDate) || Lt
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.EffectiveDate) && Lt
              (import.ContractorFeeInformation.EffectiveDate,
              entities.ContractorFeeInformation.DiscontinueDate) || Equal
              (entities.ContractorFeeInformation.DiscontinueDate,
              import.ContractorFeeInformation.EffectiveDate) || Equal
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.DiscontinueDate) || Equal
              (entities.ContractorFeeInformation.EffectiveDate,
              import.ContractorFeeInformation.EffectiveDate) || Equal
              (entities.ContractorFeeInformation.DiscontinueDate,
              import.ContractorFeeInformation.DiscontinueDate))
            {
              ExitState = "ACO_NE0000_DATE_OVERLAP";

              return;
            }
            else
            {
              continue;
            }
          }
        }

        local.Update.Flag = "Y";
      }

      if (AsChar(local.Update.Flag) == 'Y')
      {
        if (ReadContractorFeeInformation1())
        {
          try
          {
            UpdateContractorFeeInformation();
            export.ContractorFeeInformation.Assign(
              entities.ContractorFeeInformation);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                break;
              case ErrorCode.PermittedValueViolation:
                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          ExitState = "FN0000_COLL_FOR_SEL_REC_NF";
        }
      }
    }
  }

  private static void MoveContractorFeeInformation(
    ContractorFeeInformation source, ContractorFeeInformation target)
  {
    target.Rate = source.Rate;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private bool ReadContractorFeeInformation1()
  {
    entities.ContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "venIdentifier",
          import.ContractorFeeInformation.SystemGeneratedIdentifier);
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.ContractorFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ContractorFeeInformation.CreatedBy = db.GetString(reader, 4);
        entities.ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.ContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation2()
  {
    entities.Test1ContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation2",
      (db, command) =>
      {
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          import.ContractorFeeInformation.DistributionProgramType ?? "");
      },
      (db, reader) =>
      {
        entities.Test1ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Test1ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.Test1ContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.Test1ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.Test1ContractorFeeInformation.CreatedBy =
          db.GetString(reader, 4);
        entities.Test1ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Test1ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Test1ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.Test1ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.Test1ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.Test1ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.Test1ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.Test1ContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation3()
  {
    entities.Test1ContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation3",
      (db, command) =>
      {
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
      },
      (db, reader) =>
      {
        entities.Test1ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Test1ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.Test1ContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.Test1ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.Test1ContractorFeeInformation.CreatedBy =
          db.GetString(reader, 4);
        entities.Test1ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Test1ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Test1ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.Test1ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.Test1ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.Test1ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.Test1ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.Test1ContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation4()
  {
    entities.Test2ContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          import.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Test2ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Test2ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.Test2ContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.Test2ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.Test2ContractorFeeInformation.CreatedBy =
          db.GetString(reader, 4);
        entities.Test2ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Test2ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Test2ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.Test2ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.Test2ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.Test2ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.Test2ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.Test2ContractorFeeInformation.Populated = true;
      });
  }

  private bool ReadContractorFeeInformation5()
  {
    entities.Test2ContractorFeeInformation.Populated = false;

    return Read("ReadContractorFeeInformation5",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Test2ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Test2ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.Test2ContractorFeeInformation.EffectiveDate =
          db.GetDate(reader, 2);
        entities.Test2ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.Test2ContractorFeeInformation.CreatedBy =
          db.GetString(reader, 4);
        entities.Test2ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.Test2ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.Test2ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.Test2ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.Test2ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.Test2ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.Test2ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.Test2ContractorFeeInformation.Populated = true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformation6()
  {
    entities.ContractorFeeInformation.Populated = false;

    return ReadEach("ReadContractorFeeInformation6",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          import.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetInt32(
          command, "venIdentifier",
          import.ContractorFeeInformation.SystemGeneratedIdentifier);
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.ContractorFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ContractorFeeInformation.CreatedBy = db.GetString(reader, 4);
        entities.ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.ContractorFeeInformation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformationObligationType1()
  {
    entities.ObligationType.Populated = false;
    entities.ContractorFeeInformation.Populated = false;

    return ReadEach("ReadContractorFeeInformationObligationType1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetNullableString(
          command, "distPgmType",
          import.ContractorFeeInformation.DistributionProgramType ?? "");
        db.SetInt32(
          command, "venIdentifier",
          import.ContractorFeeInformation.SystemGeneratedIdentifier);
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.ContractorFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ContractorFeeInformation.CreatedBy = db.GetString(reader, 4);
        entities.ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Name = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 15);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 16);
        entities.ObligationType.CreatedBy = db.GetString(reader, 17);
        entities.ObligationType.CreatedTmst = db.GetDateTime(reader, 18);
        entities.ObligationType.LastUpdatedBy =
          db.GetNullableString(reader, 19);
        entities.ObligationType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 20);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 21);
        entities.ObligationType.Description = db.GetNullableString(reader, 22);
        entities.ObligationType.Populated = true;
        entities.ContractorFeeInformation.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadContractorFeeInformationObligationType2()
  {
    entities.ObligationType.Populated = false;
    entities.ContractorFeeInformation.Populated = false;

    return ReadEach("ReadContractorFeeInformationObligationType2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "judicialDistrict",
          import.ContractorFeeInformation.JudicialDistrict ?? "");
        db.SetInt32(
          command, "venIdentifier",
          import.ContractorFeeInformation.SystemGeneratedIdentifier);
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
        db.SetInt32(command, "offId", entities.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ContractorFeeInformation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ContractorFeeInformation.Rate = db.GetDecimal(reader, 1);
        entities.ContractorFeeInformation.EffectiveDate = db.GetDate(reader, 2);
        entities.ContractorFeeInformation.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.ContractorFeeInformation.CreatedBy = db.GetString(reader, 4);
        entities.ContractorFeeInformation.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.ContractorFeeInformation.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ContractorFeeInformation.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.ContractorFeeInformation.DistributionProgramType =
          db.GetNullableString(reader, 8);
        entities.ContractorFeeInformation.JudicialDistrict =
          db.GetNullableString(reader, 9);
        entities.ContractorFeeInformation.OffId = db.GetInt32(reader, 10);
        entities.ContractorFeeInformation.OtyId =
          db.GetNullableInt32(reader, 11);
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 11);
        entities.ObligationType.Code = db.GetString(reader, 12);
        entities.ObligationType.Name = db.GetString(reader, 13);
        entities.ObligationType.Classification = db.GetString(reader, 14);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 15);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 16);
        entities.ObligationType.CreatedBy = db.GetString(reader, 17);
        entities.ObligationType.CreatedTmst = db.GetDateTime(reader, 18);
        entities.ObligationType.LastUpdatedBy =
          db.GetNullableString(reader, 19);
        entities.ObligationType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 20);
        entities.ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 21);
        entities.ObligationType.Description = db.GetNullableString(reader, 22);
        entities.ObligationType.Populated = true;
        entities.ContractorFeeInformation.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private bool ReadObligationType1()
  {
    System.Diagnostics.Debug.Assert(
      entities.Test1ContractorFeeInformation.Populated);
    entities.Test1ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.Test1ContractorFeeInformation.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Test1ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Test1ObligationType.Code = db.GetString(reader, 1);
        entities.Test1ObligationType.Name = db.GetString(reader, 2);
        entities.Test1ObligationType.Classification = db.GetString(reader, 3);
        entities.Test1ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.Test1ObligationType.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.Test1ObligationType.CreatedBy = db.GetString(reader, 6);
        entities.Test1ObligationType.CreatedTmst = db.GetDateTime(reader, 7);
        entities.Test1ObligationType.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.Test1ObligationType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.Test1ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 10);
        entities.Test1ObligationType.Description =
          db.GetNullableString(reader, 11);
        entities.Test1ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.Test1ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.Test1ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(
      entities.Test2ContractorFeeInformation.Populated);
    entities.Test2ObligationType.Populated = false;

    return Read("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.Test2ContractorFeeInformation.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Test2ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.Test2ObligationType.Code = db.GetString(reader, 1);
        entities.Test2ObligationType.Name = db.GetString(reader, 2);
        entities.Test2ObligationType.Classification = db.GetString(reader, 3);
        entities.Test2ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.Test2ObligationType.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.Test2ObligationType.CreatedBy = db.GetString(reader, 6);
        entities.Test2ObligationType.CreatedTmst = db.GetDateTime(reader, 7);
        entities.Test2ObligationType.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.Test2ObligationType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.Test2ObligationType.SupportedPersonReqInd =
          db.GetString(reader, 10);
        entities.Test2ObligationType.Description =
          db.GetNullableString(reader, 11);
        entities.Test2ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.Test2ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.Test2ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(command, "officeId", import.Office.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.MainPhoneNumber = db.GetNullableInt32(reader, 1);
        entities.Office.MainFaxPhoneNumber = db.GetNullableInt32(reader, 2);
        entities.Office.TypeCode = db.GetString(reader, 3);
        entities.Office.Name = db.GetString(reader, 4);
        entities.Office.LastUpdatedBy = db.GetNullableString(reader, 5);
        entities.Office.LastUpdatdTstamp = db.GetNullableDateTime(reader, 6);
        entities.Office.CreatedBy = db.GetString(reader, 7);
        entities.Office.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Office.EffectiveDate = db.GetDate(reader, 9);
        entities.Office.DiscontinueDate = db.GetNullableDate(reader, 10);
        entities.Office.MainPhoneAreaCode = db.GetNullableInt32(reader, 11);
        entities.Office.MainFaxAreaCode = db.GetNullableInt32(reader, 12);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 13);
        entities.Office.Populated = true;
      });
  }

  private void UpdateContractorFeeInformation()
  {
    System.Diagnostics.Debug.
      Assert(entities.ContractorFeeInformation.Populated);

    var rate = export.ContractorFeeInformation.Rate;
    var effectiveDate = export.ContractorFeeInformation.EffectiveDate;
    var discontinueDate = export.ContractorFeeInformation.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.ContractorFeeInformation.Populated = false;
    Update("UpdateContractorFeeInformation",
      (db, command) =>
      {
        db.SetDecimal(command, "rate", rate);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "venIdentifier",
          entities.ContractorFeeInformation.SystemGeneratedIdentifier);
        db.SetInt32(command, "offId", entities.ContractorFeeInformation.OffId);
      });

    entities.ContractorFeeInformation.Rate = rate;
    entities.ContractorFeeInformation.EffectiveDate = effectiveDate;
    entities.ContractorFeeInformation.DiscontinueDate = discontinueDate;
    entities.ContractorFeeInformation.LastUpdatedBy = lastUpdatedBy;
    entities.ContractorFeeInformation.LastUpdatedTmst = lastUpdatedTmst;
    entities.ContractorFeeInformation.Populated = true;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
    }

    private ObligationType obligationType;
    private Office office;
    private ContractorFeeInformation contractorFeeInformation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
    }

    private ContractorFeeInformation contractorFeeInformation;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public Common Update
    {
      get => update ??= new();
      set => update = value;
    }

    private Common update;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Test2ObligationType.
    /// </summary>
    [JsonPropertyName("test2ObligationType")]
    public ObligationType Test2ObligationType
    {
      get => test2ObligationType ??= new();
      set => test2ObligationType = value;
    }

    /// <summary>
    /// A value of Test1ObligationType.
    /// </summary>
    [JsonPropertyName("test1ObligationType")]
    public ObligationType Test1ObligationType
    {
      get => test1ObligationType ??= new();
      set => test1ObligationType = value;
    }

    /// <summary>
    /// A value of Test2ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("test2ContractorFeeInformation")]
    public ContractorFeeInformation Test2ContractorFeeInformation
    {
      get => test2ContractorFeeInformation ??= new();
      set => test2ContractorFeeInformation = value;
    }

    /// <summary>
    /// A value of Test1ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("test1ContractorFeeInformation")]
    public ContractorFeeInformation Test1ContractorFeeInformation
    {
      get => test1ContractorFeeInformation ??= new();
      set => test1ContractorFeeInformation = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ContractorFeeInformation.
    /// </summary>
    [JsonPropertyName("contractorFeeInformation")]
    public ContractorFeeInformation ContractorFeeInformation
    {
      get => contractorFeeInformation ??= new();
      set => contractorFeeInformation = value;
    }

    private ObligationType test2ObligationType;
    private ObligationType test1ObligationType;
    private ContractorFeeInformation test2ContractorFeeInformation;
    private ContractorFeeInformation test1ContractorFeeInformation;
    private Office office;
    private ObligationType obligationType;
    private ContractorFeeInformation contractorFeeInformation;
  }
#endregion
}
