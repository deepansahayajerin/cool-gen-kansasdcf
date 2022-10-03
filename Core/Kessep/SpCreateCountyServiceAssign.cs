// Program: SP_CREATE_COUNTY_SERVICE_ASSIGN, ID: 371781015, model: 746.
// Short name: SWE01308
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CREATE_COUNTY_SERVICE_ASSIGN.
/// </summary>
[Serializable]
public partial class SpCreateCountyServiceAssign: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CREATE_COUNTY_SERVICE_ASSIGN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCreateCountyServiceAssign(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCreateCountyServiceAssign.
  /// </summary>
  public SpCreateCountyServiceAssign(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************
    // **
    // **  M A I N T E N A N C E   L O G
    // **
    // **  Date	Description
    // **  5/95	Rod Grey        Update
    // **  2/97	Rod Grey	Add processing for Function
    // **
    // 
    // type County Service
    // **  4/30/97	Rod Grey	Change Current Date
    // **
    // ********************************************
    export.Office.SystemGeneratedId = import.Office.SystemGeneratedId;
    MoveCseOrganization(import.Import1.CseOrganization,
      export.Export1.CseOrganization);
    export.Export1.CountyService.Assign(import.Import1.CountyService);
    export.Export1.Program.Assign(import.Import1.Program);
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";

    // ************************************************
    // * Validate Office and CSE Organization Codes.
    // ************************************************
    if (ReadOffice())
    {
      export.Office.SystemGeneratedId = entities.Office.SystemGeneratedId;
    }
    else
    {
      ExitState = "FN0000_OFFICE_NF";

      return;
    }

    if (!ReadCseOrganization())
    {
      ExitState = "INVALID_CSE_ORG";

      return;
    }

    // **********************************************
    // * Determine if single or multiple County Service Add.
    // **********************************************
    switch(AsChar(export.Export1.CountyService.Type1))
    {
      case 'P':
        if (Equal(export.Export1.Program.Code, "*"))
        {
          local.MultipleAdd.Flag = "Y";
        }
        else
        {
          local.MultipleAdd.Flag = "N";
        }

        break;
      case 'F':
        if (Equal(export.Export1.CountyService.Function, "*"))
        {
          local.MultipleAdd.Flag = "Y";
        }
        else
        {
          local.MultipleAdd.Flag = "N";
        }

        break;
      default:
        break;
    }

    if (AsChar(local.MultipleAdd.Flag) == 'Y')
    {
      // ********************************************
      // * If County Service not Already Exist, create.
      // * Multiple Service Add for single county (*)
      // ********************************************
      switch(AsChar(export.Export1.CountyService.Type1))
      {
        case 'P':
          if (Equal(export.Export1.Program.Code, "*"))
          {
            export.AllPgm.Index = 0;
            export.AllPgm.Clear();

            foreach(var item in ReadProgram2())
            {
              export.AllPgm.Update.AlProgram.Assign(entities.Program);

              // *********************************************
              // * Determine if County Services assignments already exist
              // * for any Programs (MULTIPLE ADD)
              // *********************************************
              if (ReadCountyService1())
              {
                ExitState = "COUNTY_SERVICES_AE";

                // *******************************************
                // * Skip over processing for create and fetch
                // * next program for county service. (For
                // * multiple program adds.)
                // *******************************************
              }
              else
              {
                // *******************************************
                // * Process first time and loop.
                // ********************************************
                // *****************************************
                // * Get SYS-GENERATED-ID for new County Service Program
                // *****************************************
                export.Export1.ControlTable.Identifier = "COUNTY SERVICE";

                if (ReadControlTable())
                {
                  MoveControlTable(entities.ControlTable,
                    export.Export1.ControlTable);

                  try
                  {
                    UpdateControlTable();
                    ExitState = "ACO_NN0000_ALL_OK";

                    try
                    {
                      CreateCountyService1();
                      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
                    }
                    catch(Exception e1)
                    {
                      switch(GetErrorCode(e1))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "COUNTY_SERVICES_AE";

                          break;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "COUNTY_SERVICES_PV";

                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "CONTROL_TABLE_VALUE_NU";
                        export.AllPgm.Next();

                        return;
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
                  ExitState = "CONTROL_TABLE_ID_NF";
                  export.AllPgm.Next();

                  return;
                }
              }

              export.AllPgm.Next();
            }
          }

          break;
        case 'F':
          if (Equal(export.Export1.CountyService.Function, "*"))
          {
            ReadCode();

            export.Cdvl.Index = 0;
            export.Cdvl.Clear();

            foreach(var item in ReadCodeValue())
            {
              // *********************************************
              // * Determine if County Services assignments already exist
              // * for any Functions (MULTIPLE ADD)
              // *********************************************
              if (ReadCountyService2())
              {
                ExitState = "COUNTY_SERVICES_AE";

                // *******************************************
                // * Skip over processing for create and fetch
                // * next program for county service. (For
                // * multiple program adds.)
                // *******************************************
              }
              else
              {
                // *******************************************
                // * Continue ADD processing.
                // * Process first time and loop.
                // ********************************************
                export.Export1.ControlTable.Identifier = "COUNTY SERVICE";

                // *****************************************
                // * Get SYS-GENERATED-ID for new County Service Function
                // *****************************************
                if (ReadControlTable())
                {
                  MoveControlTable(entities.ControlTable,
                    export.Export1.ControlTable);

                  try
                  {
                    UpdateControlTable();
                    ExitState = "ACO_NN0000_ALL_OK";

                    try
                    {
                      CreateCountyService2();
                      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
                    }
                    catch(Exception e1)
                    {
                      switch(GetErrorCode(e1))
                      {
                        case ErrorCode.AlreadyExists:
                          ExitState = "COUNTY_SERVICES_AE";

                          break;
                        case ErrorCode.PermittedValueViolation:
                          ExitState = "COUNTY_SERVICES_PV";

                          break;
                        case ErrorCode.DatabaseError:
                          break;
                        default:
                          throw;
                      }
                    }
                  }
                  catch(Exception e)
                  {
                    switch(GetErrorCode(e))
                    {
                      case ErrorCode.AlreadyExists:
                        ExitState = "CONTROL_TABLE_VALUE_NU";
                        export.Cdvl.Next();

                        return;
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
                  ExitState = "CONTROL_TABLE_ID_NF";
                  export.Cdvl.Next();

                  return;
                }
              }

              export.Cdvl.Next();
            }
          }

          break;
        default:
          break;
      }
    }
    else if (AsChar(local.MultipleAdd.Flag) == 'N')
    {
      // *****************************************
      // * Get SYS-GENERATED-ID for County Service program
      // *****************************************
      export.Export1.ControlTable.Identifier = "COUNTY SERVICE";

      if (ReadControlTable())
      {
        MoveControlTable(entities.ControlTable, export.Export1.ControlTable);

        try
        {
          UpdateControlTable();
          ExitState = "ACO_NN0000_ALL_OK";

          if (AsChar(export.Export1.CountyService.Type1) == 'P')
          {
            // *********************************************
            // * Determine if County Services already exist.
            // * (FOR SINGLE ADD)
            // *********************************************
            if (ReadProgram1())
            {
              if (ReadCountyService4())
              {
                ExitState = "COUNTY_SERVICES_AE";
              }
              else
              {
                // ********************************************
                // * Continue single County Service ADD request
                // * for PROGRAM Type County Service.
                // *********************************************
                try
                {
                  CreateCountyService4();
                  MoveCountyService(entities.CountyService,
                    export.Export1.CountyService);
                  export.Export1.CountyService.Function = "";
                  ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
                }
                catch(Exception e1)
                {
                  switch(GetErrorCode(e1))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "COUNTY_SERVICES_AE";

                      break;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "COUNTY_SERVICES_PV";

                      break;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }
            else
            {
              ExitState = "INVALID_PROGRAM";
            }
          }
          else if (AsChar(export.Export1.CountyService.Type1) == 'F')
          {
            if (ReadCountyService3())
            {
              ExitState = "COUNTY_SERVICES_AE";
            }
            else
            {
              // ********************************************
              // * Continue single County Service ADD request
              // * for FUNCTION Type County Service.
              // *********************************************
              try
              {
                CreateCountyService3();
                MoveCountyService(entities.CountyService,
                  export.Export1.CountyService);
                export.Export1.Program.Code = "";
                ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "COUNTY_SERVICES_AE";

                    break;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "COUNTY_SERVICES_PV";

                    break;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CONTROL_TABLE_VALUE_NU";

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
        ExitState = "CONTROL_TABLE_ID_NF";
      }
    }
  }

  private static void MoveControlTable(ControlTable source, ControlTable target)
  {
    target.Identifier = source.Identifier;
    target.LastUsedNumber = source.LastUsedNumber;
  }

  private static void MoveCountyService(CountyService source,
    CountyService target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.Function = source.Function;
  }

  private static void MoveCseOrganization(CseOrganization source,
    CseOrganization target)
  {
    target.Code = source.Code;
    target.Type1 = source.Type1;
  }

  private void CreateCountyService1()
  {
    var systemGeneratedIdentifier = export.Export1.ControlTable.LastUsedNumber;
    var type1 = export.Export1.CountyService.Type1;
    var effectiveDate = export.Export1.CountyService.EffectiveDate;
    var discontinueDate = export.Export1.CountyService.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatdTstamp = Now();
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var cogTypeCode = entities.CseOrganization.Type1;
    var cogCode = entities.CseOrganization.Code;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.CountyService.Populated = false;
    Update("CreateCountyService1",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratdId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatdTstamp", lastUpdatdTstamp);
        db.SetString(command, "createdBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", lastUpdatdTstamp);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "cogTypeCode", cogTypeCode);
        db.SetNullableString(command, "cogCode", cogCode);
        db.SetNullableString(command, "function", "");
        db.SetNullableInt32(command, "prgGeneratedId", prgGeneratedId);
      });

    entities.CountyService.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CountyService.Type1 = type1;
    entities.CountyService.EffectiveDate = effectiveDate;
    entities.CountyService.DiscontinueDate = discontinueDate;
    entities.CountyService.LastUpdatedBy = lastUpdatedBy;
    entities.CountyService.LastUpdatdTstamp = lastUpdatdTstamp;
    entities.CountyService.CreatedBy = lastUpdatedBy;
    entities.CountyService.CreatedTimestamp = lastUpdatdTstamp;
    entities.CountyService.OffGeneratedId = offGeneratedId;
    entities.CountyService.CogTypeCode = cogTypeCode;
    entities.CountyService.CogCode = cogCode;
    entities.CountyService.Function = "";
    entities.CountyService.PrgGeneratedId = prgGeneratedId;
    entities.CountyService.Populated = true;
  }

  private void CreateCountyService2()
  {
    var systemGeneratedIdentifier = export.Export1.ControlTable.LastUsedNumber;
    var type1 = export.Export1.CountyService.Type1;
    var effectiveDate = export.Export1.CountyService.EffectiveDate;
    var discontinueDate = export.Export1.CountyService.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var cogTypeCode = entities.CseOrganization.Type1;
    var cogCode = entities.CseOrganization.Code;
    var function = Substring(entities.ExistingCodeValue.Cdvalue, 1, 3);

    entities.CountyService.Populated = false;
    Update("CreateCountyService2",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratdId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "cogTypeCode", cogTypeCode);
        db.SetNullableString(command, "cogCode", cogCode);
        db.SetNullableString(command, "function", function);
      });

    entities.CountyService.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CountyService.Type1 = type1;
    entities.CountyService.EffectiveDate = effectiveDate;
    entities.CountyService.DiscontinueDate = discontinueDate;
    entities.CountyService.LastUpdatedBy = "";
    entities.CountyService.LastUpdatdTstamp = null;
    entities.CountyService.CreatedBy = createdBy;
    entities.CountyService.CreatedTimestamp = createdTimestamp;
    entities.CountyService.OffGeneratedId = offGeneratedId;
    entities.CountyService.CogTypeCode = cogTypeCode;
    entities.CountyService.CogCode = cogCode;
    entities.CountyService.Function = function;
    entities.CountyService.PrgGeneratedId = null;
    entities.CountyService.Populated = true;
  }

  private void CreateCountyService3()
  {
    var systemGeneratedIdentifier = export.Export1.ControlTable.LastUsedNumber;
    var type1 = export.Export1.CountyService.Type1;
    var effectiveDate = export.Export1.CountyService.EffectiveDate;
    var discontinueDate = export.Export1.CountyService.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var cogTypeCode = entities.CseOrganization.Type1;
    var cogCode = entities.CseOrganization.Code;
    var function = export.Export1.CountyService.Function ?? "";

    entities.CountyService.Populated = false;
    Update("CreateCountyService3",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratdId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "cogTypeCode", cogTypeCode);
        db.SetNullableString(command, "cogCode", cogCode);
        db.SetNullableString(command, "function", function);
      });

    entities.CountyService.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CountyService.Type1 = type1;
    entities.CountyService.EffectiveDate = effectiveDate;
    entities.CountyService.DiscontinueDate = discontinueDate;
    entities.CountyService.LastUpdatedBy = "";
    entities.CountyService.LastUpdatdTstamp = null;
    entities.CountyService.CreatedBy = createdBy;
    entities.CountyService.CreatedTimestamp = createdTimestamp;
    entities.CountyService.OffGeneratedId = offGeneratedId;
    entities.CountyService.CogTypeCode = cogTypeCode;
    entities.CountyService.CogCode = cogCode;
    entities.CountyService.Function = function;
    entities.CountyService.PrgGeneratedId = null;
    entities.CountyService.Populated = true;
  }

  private void CreateCountyService4()
  {
    var systemGeneratedIdentifier = export.Export1.ControlTable.LastUsedNumber;
    var type1 = export.Export1.CountyService.Type1;
    var effectiveDate = export.Export1.CountyService.EffectiveDate;
    var discontinueDate = export.Export1.CountyService.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var offGeneratedId = entities.Office.SystemGeneratedId;
    var cogTypeCode = entities.CseOrganization.Type1;
    var cogCode = entities.CseOrganization.Code;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;

    entities.CountyService.Populated = false;
    Update("CreateCountyService4",
      (db, command) =>
      {
        db.SetInt32(command, "systemGeneratdId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatdTstamp", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableString(command, "cogTypeCode", cogTypeCode);
        db.SetNullableString(command, "cogCode", cogCode);
        db.SetNullableString(command, "function", "");
        db.SetNullableInt32(command, "prgGeneratedId", prgGeneratedId);
      });

    entities.CountyService.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CountyService.Type1 = type1;
    entities.CountyService.EffectiveDate = effectiveDate;
    entities.CountyService.DiscontinueDate = discontinueDate;
    entities.CountyService.LastUpdatedBy = "";
    entities.CountyService.LastUpdatdTstamp = null;
    entities.CountyService.CreatedBy = createdBy;
    entities.CountyService.CreatedTimestamp = createdTimestamp;
    entities.CountyService.OffGeneratedId = offGeneratedId;
    entities.CountyService.CogTypeCode = cogTypeCode;
    entities.CountyService.CogCode = cogCode;
    entities.CountyService.Function = "";
    entities.CountyService.PrgGeneratedId = prgGeneratedId;
    entities.CountyService.Populated = true;
  }

  private bool ReadCode()
  {
    entities.ExistingCode.Populated = false;

    return Read("ReadCode",
      null,
      (db, reader) =>
      {
        entities.ExistingCode.Id = db.GetInt32(reader, 0);
        entities.ExistingCode.CodeName = db.GetString(reader, 1);
        entities.ExistingCode.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCodeValue()
  {
    return ReadEach("ReadCodeValue",
      (db, command) =>
      {
        db.SetNullableInt32(command, "codId", entities.ExistingCode.Id);
      },
      (db, reader) =>
      {
        if (export.Cdvl.IsFull)
        {
          return false;
        }

        entities.ExistingCodeValue.Id = db.GetInt32(reader, 0);
        entities.ExistingCodeValue.CodId = db.GetNullableInt32(reader, 1);
        entities.ExistingCodeValue.Cdvalue = db.GetString(reader, 2);
        entities.ExistingCodeValue.EffectiveDate = db.GetDate(reader, 3);
        entities.ExistingCodeValue.ExpirationDate = db.GetDate(reader, 4);
        entities.ExistingCodeValue.Populated = true;

        return true;
      });
  }

  private bool ReadControlTable()
  {
    entities.ControlTable.Populated = false;

    return Read("ReadControlTable",
      (db, command) =>
      {
        db.SetString(
          command, "cntlTblId", export.Export1.ControlTable.Identifier);
      },
      (db, reader) =>
      {
        entities.ControlTable.Identifier = db.GetString(reader, 0);
        entities.ControlTable.LastUsedNumber = db.GetInt32(reader, 1);
        entities.ControlTable.Populated = true;
      });
  }

  private bool ReadCountyService1()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetString(command, "code", export.AllPgm.Item.AlProgram.Code);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CountyService.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.CountyService.CreatedBy = db.GetString(reader, 6);
        entities.CountyService.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 8);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 9);
        entities.CountyService.CogCode = db.GetNullableString(reader, 10);
        entities.CountyService.Function = db.GetNullableString(reader, 11);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 12);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadCountyService2()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetString(command, "cdvalue", entities.ExistingCodeValue.Cdvalue);
        db.SetDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CountyService.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.CountyService.CreatedBy = db.GetString(reader, 6);
        entities.CountyService.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 8);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 9);
        entities.CountyService.CogCode = db.GetNullableString(reader, 10);
        entities.CountyService.Function = db.GetNullableString(reader, 11);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 12);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadCountyService3()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetNullableString(
          command, "function", export.Export1.CountyService.Function ?? "");
        db.SetDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CountyService.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.CountyService.CreatedBy = db.GetString(reader, 6);
        entities.CountyService.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 8);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 9);
        entities.CountyService.CogCode = db.GetNullableString(reader, 10);
        entities.CountyService.Function = db.GetNullableString(reader, 11);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 12);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadCountyService4()
  {
    entities.CountyService.Populated = false;

    return Read("ReadCountyService4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetNullableString(
          command, "cogTypeCode", entities.CseOrganization.Type1);
        db.SetNullableString(command, "cogCode", entities.CseOrganization.Code);
        db.SetNullableInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.CountyService.LastUpdatdTstamp =
          db.GetNullableDateTime(reader, 5);
        entities.CountyService.CreatedBy = db.GetString(reader, 6);
        entities.CountyService.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 8);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 9);
        entities.CountyService.CogCode = db.GetNullableString(reader, 10);
        entities.CountyService.Function = db.GetNullableString(reader, 11);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 12);
        entities.CountyService.Populated = true;
      });
  }

  private bool ReadCseOrganization()
  {
    entities.CseOrganization.Populated = false;

    return Read("ReadCseOrganization",
      (db, command) =>
      {
        db.SetString(command, "typeCode", export.Export1.CseOrganization.Type1);
        db.
          SetString(command, "organztnId", export.Export1.CseOrganization.Code);
          
      },
      (db, reader) =>
      {
        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Populated = true;
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
        entities.Office.OffOffice = db.GetNullableInt32(reader, 1);
        entities.Office.Populated = true;
      });
  }

  private bool ReadProgram1()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetString(command, "code", export.Export1.Program.Code);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.DistributionProgramType = db.GetString(reader, 2);
        entities.Program.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram2()
  {
    return ReadEach("ReadProgram2",
      null,
      (db, reader) =>
      {
        if (export.AllPgm.IsFull)
        {
          return false;
        }

        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.DistributionProgramType = db.GetString(reader, 2);
        entities.Program.Populated = true;

        return true;
      });
  }

  private void UpdateControlTable()
  {
    var lastUsedNumber = export.Export1.ControlTable.LastUsedNumber + 1;

    entities.ControlTable.Populated = false;
    Update("UpdateControlTable",
      (db, command) =>
      {
        db.SetInt32(command, "lastUsedNumber", lastUsedNumber);
        db.SetString(command, "cntlTblId", entities.ControlTable.Identifier);
      });

    entities.ControlTable.LastUsedNumber = lastUsedNumber;
    entities.ControlTable.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of CseOrganization.
      /// </summary>
      [JsonPropertyName("cseOrganization")]
      public CseOrganization CseOrganization
      {
        get => cseOrganization ??= new();
        set => cseOrganization = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of CountyService.
      /// </summary>
      [JsonPropertyName("countyService")]
      public CountyService CountyService
      {
        get => countyService ??= new();
        set => countyService = value;
      }

      private CseOrganization cseOrganization;
      private Program program;
      private CountyService countyService;
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
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonPropertyName("import1")]
    public ImportGroup Import1
    {
      get => import1 ?? (import1 = new());
      set => import1 = value;
    }

    private Office office;
    private ControlTable controlTable;
    private ImportGroup import1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of ControlTable.
      /// </summary>
      [JsonPropertyName("controlTable")]
      public ControlTable ControlTable
      {
        get => controlTable ??= new();
        set => controlTable = value;
      }

      /// <summary>
      /// A value of CseOrganization.
      /// </summary>
      [JsonPropertyName("cseOrganization")]
      public CseOrganization CseOrganization
      {
        get => cseOrganization ??= new();
        set => cseOrganization = value;
      }

      /// <summary>
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

      /// <summary>
      /// A value of CountyService.
      /// </summary>
      [JsonPropertyName("countyService")]
      public CountyService CountyService
      {
        get => countyService ??= new();
        set => countyService = value;
      }

      private ControlTable controlTable;
      private CseOrganization cseOrganization;
      private Program program;
      private CountyService countyService;
    }

    /// <summary>A AllPgmGroup group.</summary>
    [Serializable]
    public class AllPgmGroup
    {
      /// <summary>
      /// A value of All.
      /// </summary>
      [JsonPropertyName("all")]
      public ControlTable All
      {
        get => all ??= new();
        set => all = value;
      }

      /// <summary>
      /// A value of PgmAl.
      /// </summary>
      [JsonPropertyName("pgmAl")]
      public CseOrganization PgmAl
      {
        get => pgmAl ??= new();
        set => pgmAl = value;
      }

      /// <summary>
      /// A value of AlProgram.
      /// </summary>
      [JsonPropertyName("alProgram")]
      public Program AlProgram
      {
        get => alProgram ??= new();
        set => alProgram = value;
      }

      /// <summary>
      /// A value of AlCountyService.
      /// </summary>
      [JsonPropertyName("alCountyService")]
      public CountyService AlCountyService
      {
        get => alCountyService ??= new();
        set => alCountyService = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 30;

      private ControlTable all;
      private CseOrganization pgmAl;
      private Program alProgram;
      private CountyService alCountyService;
    }

    /// <summary>A CdvlGroup group.</summary>
    [Serializable]
    public class CdvlGroup
    {
      /// <summary>
      /// A value of ReturnVal.
      /// </summary>
      [JsonPropertyName("returnVal")]
      public Common ReturnVal
      {
        get => returnVal ??= new();
        set => returnVal = value;
      }

      /// <summary>
      /// A value of Code.
      /// </summary>
      [JsonPropertyName("code")]
      public Code Code
      {
        get => code ??= new();
        set => code = value;
      }

      /// <summary>
      /// A value of CodeValue.
      /// </summary>
      [JsonPropertyName("codeValue")]
      public CodeValue CodeValue
      {
        get => codeValue ??= new();
        set => codeValue = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common returnVal;
      private Code code;
      private CodeValue codeValue;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonPropertyName("export1")]
    public ExportGroup Export1
    {
      get => export1 ?? (export1 = new());
      set => export1 = value;
    }

    /// <summary>
    /// Gets a value of AllPgm.
    /// </summary>
    [JsonIgnore]
    public Array<AllPgmGroup> AllPgm => allPgm ??= new(AllPgmGroup.Capacity);

    /// <summary>
    /// Gets a value of AllPgm for json serialization.
    /// </summary>
    [JsonPropertyName("allPgm")]
    [Computed]
    public IList<AllPgmGroup> AllPgm_Json
    {
      get => allPgm;
      set => AllPgm.Assign(value);
    }

    /// <summary>
    /// Gets a value of Cdvl.
    /// </summary>
    [JsonIgnore]
    public Array<CdvlGroup> Cdvl => cdvl ??= new(CdvlGroup.Capacity);

    /// <summary>
    /// Gets a value of Cdvl for json serialization.
    /// </summary>
    [JsonPropertyName("cdvl")]
    [Computed]
    public IList<CdvlGroup> Cdvl_Json
    {
      get => cdvl;
      set => Cdvl.Assign(value);
    }

    private Office office;
    private ExportGroup export1;
    private Array<AllPgmGroup> allPgm;
    private Array<CdvlGroup> cdvl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of MultipleAdd.
    /// </summary>
    [JsonPropertyName("multipleAdd")]
    public Common MultipleAdd
    {
      get => multipleAdd ??= new();
      set => multipleAdd = value;
    }

    private DateWorkArea current;
    private CodeValue codeValue;
    private Common multipleAdd;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCode.
    /// </summary>
    [JsonPropertyName("existingCode")]
    public Code ExistingCode
    {
      get => existingCode ??= new();
      set => existingCode = value;
    }

    /// <summary>
    /// A value of ExistingCodeValue.
    /// </summary>
    [JsonPropertyName("existingCodeValue")]
    public CodeValue ExistingCodeValue
    {
      get => existingCodeValue ??= new();
      set => existingCodeValue = value;
    }

    /// <summary>
    /// A value of ControlTable.
    /// </summary>
    [JsonPropertyName("controlTable")]
    public ControlTable ControlTable
    {
      get => controlTable ??= new();
      set => controlTable = value;
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
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
    }

    private Code existingCode;
    private CodeValue existingCodeValue;
    private ControlTable controlTable;
    private Office office;
    private CseOrganization cseOrganization;
    private Program program;
    private CountyService countyService;
  }
#endregion
}
