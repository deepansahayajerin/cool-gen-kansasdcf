// Program: SP_DTR_AP_LOCATE_STATUS, ID: 372070766, model: 746.
// Short name: SWE02122
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DTR_AP_LOCATE_STATUS.
/// </para>
/// <para>
/// This common action block determines what events should be raised to 
/// manipulate the STATE of a newly created Case Unit.
/// </para>
/// </summary>
[Serializable]
public partial class SpDtrApLocateStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DTR_AP_LOCATE_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDtrApLocateStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDtrApLocateStatus.
  /// </summary>
  public SpDtrApLocateStatus(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // Initial development - September 30, 1997
    // Developer - Jack Rookard, MTW
    // This action block is invoked by the Event Processor when a
    // "AP un-located" type event is encountered.
    // It determines the actual locate status of the AP participating
    // in the imported Case Unit and raises Infrastructure
    // occurrences as appropriate.
    // 11/03/00		SWSRPRM
    // PR # 106919 - Incorrect creation of INCSVRFD in situations
    // where the income source is of "E" does not have the
    // appropriate return code of "E" nor "W" or "M" does not
    // have the appropriate return code of "A".
    // 05/13/2010 GVandy  CQ966  Return infrastructure records to be created in 
    // a group view rather than create in this cab.
    // ----------------------------------------------------------------
    // *****************************************************************
    // cse_person_address type changed from "F" => "M" in the
    // READ EACH for Foreign Address
    // ***********************************************
    // Crook 07Dec98 ***
    // *****************************************************************
    // cse_person_addressREAD EACH for both Foriegn and Domestic addresses
    // ************************************
    // Ketterhagen 07oct99****
    local.Current.Date = Now().Date;
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.ReferenceDate = Now().Date;
    local.Infrastructure.EventId = 10;
    local.CreateLocateInfra.Flag = "N";
    local.Infrastructure.UserId = "SWEPB301";
    local.Infrastructure.BusinessObjectCd = "CAU";
    local.Incs.Flag = "N";
    local.ApIsLocated.Flag = "N";
    UseCabConvertDate2String();

    if (ReadCase())
    {
      local.Infrastructure.CaseNumber = entities.Case1.Number;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadInterstateRequest())
    {
      local.Infrastructure.InitiatingStateCode = "OS";
    }
    else
    {
      local.Infrastructure.InitiatingStateCode = "KS";
    }

    if (ReadCaseUnit())
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      local.HoldCuNum.Text3 = NumberToString(entities.CaseUnit.CuNumber, 13, 3);
      local.CaseUnitIsLocatedFlag.Text1 =
        Substring(entities.CaseUnit.State, 3, 1);
      local.Infrastructure.SituationNumber = 0;
    }
    else
    {
      ExitState = "CASE_UNIT_NF";

      return;
    }

    if (ReadCsePerson1())
    {
      local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
    }
    else
    {
      ExitState = "CO0000_AP_CSE_PERSON_NF";

      return;
    }

    if (!ReadCsePerson2())
    {
      ExitState = "AR_DB_ERROR_NF";

      return;
    }

    if (!ReadCsePerson3())
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    // -------------------------------
    // Locate determination processing
    // -------------------------------
    // --------------------------------
    // Perform AP Date of Death inquiry
    // --------------------------------
    if (!Lt(local.Current.Date, entities.ApCsePerson.DateOfDeath) && !
      Equal(entities.ApCsePerson.DateOfDeath, local.Null1.Date))
    {
      if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
      {
        // ----------------------------------------------------------------
        // The AP for the current Case Unit was located at some time in
        // the past.  The "is Located" flag for the current Case Unit is
        // Y, which is correct since the AP is still located via a date of
        // death.
        // Do not raise any locate events.
        // ----------------------------------------------------------------
        local.ApIsLocated.Flag = "Y";

        goto Test1;
      }

      // ----------------------------------------------------------------
      // The imported Case Unit "is located" flag is N, meaning the
      // AP is not currently located.
      // The AP has now been located via a date of death. Raise the
      // appropriate event.
      // ----------------------------------------------------------------
      local.Infrastructure.ReasonCode = "APDEAD";
      local.Infrastructure.UserId = "APDS";
      local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case:";
        
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
        .Case1.Number;
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " located via dod on:";
        
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
        .TextWorkArea.Text8;
      local.ApIsLocated.Flag = "Y";
      local.CreateLocateInfra.Flag = "Y";
    }

Test1:

    // -----------------------------------
    // Perform AP domestic address inquiry
    // -----------------------------------
    // *****************************************************************
    // removed zdel_start and zdel verified code from the where clause
    // on the read each for domestic address. Added verified_date
    // <= current date. (qualification on type seems unecessary as
    // there are only m and r types for domestic left this as is)
    // ******************************************
    // Ketterhagen 07oct99
    // ******************************************************************added 
    // qualification that the verified date must be greater than the null date
    // 
    // *******************************Ketterhagen 16 Dec 99*********
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      if (ReadCsePersonAddress2())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // The AP for the current Case Unit was located at some time in the 
          // past.  The "is Located" flag
          // for the current Case Unit is Y, which is correct since the AP is 
          // still located through
          // a current domestic address. Do not raise "Is Located" events.
          local.ApIsLocated.Flag = "Y";

          goto Test2;
        }

        // -------------------------------------------------------------
        // The imported Case Unit "is located" flag is N, meaning the
        // AP is not currently located.
        // The AP has now been located via a verified address. Raise
        // the appropriate event.
        // -------------------------------------------------------------
        local.Infrastructure.ReasonCode = "ADDRVRFDAP";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
          .Case1.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " located via domestic addr on:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test2:

    // ----------------------------------
    // Perform AP foreign address inquiry
    // ----------------------------------
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      // *****************************************************************
      // cse_person_address type changed from "F" => "M" in the following
      // READ EACH for Foreign Address
      // ***********************************************
      // Crook 07Dec98 ***
      // *****************************************************************
      // removed zdel_start and zdel verified code from the where clause
      // on the read each for foreign address. Added verified_date <=
      // current date. (qualification on type seems unecessary as
      // there are only m types for foriegn left this as is)
      // ******************************************
      // Ketterhagen 07oct99
      // ******************************************************************added
      // qualification that the verified date must be greater than the null
      // date
      // 
      // *******************************Ketterhagen 16 Dec 99*********
      if (ReadCsePersonAddress1())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // --------------------------------------------------------------------
          // The AP for the current Case Unit was located at some time in
          // the past.  The "is Located" flag for the current Case Unit is
          // Y, which is correct, since the AP is still located through a
          // current foreign address. Do not raise "Is Located" events.
          // --------------------------------------------------------------------
          local.ApIsLocated.Flag = "Y";

          goto Test3;
        }

        // --------------------------------------------------------------------
        // The imported Case Unit "is located" flag is N, meaning the
        // AP is not currently located.
        // The AP has now been located via a verified foreign address.
        // Raise the appropriate event.
        // --------------------------------------------------------------------
        local.Infrastructure.ReasonCode = "FADDRVRFDAP";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
          .Case1.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " located via foreign addr on:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test3:

    // --------------------------------
    // Perform AP income source inquiry
    // --------------------------------
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      foreach(var item in ReadIncomeSource())
      {
        switch(AsChar(entities.ApIncomeSource.Type1))
        {
          case 'E':
            if (AsChar(entities.ApIncomeSource.ReturnCd) == 'E' || AsChar
              (entities.ApIncomeSource.ReturnCd) == 'W')
            {
              local.Incs.Flag = "Y";
            }

            break;
          case 'M':
            if (AsChar(entities.ApIncomeSource.ReturnCd) == 'A')
            {
              local.Incs.Flag = "Y";
            }

            break;
          default:
            continue;
        }

        if (AsChar(local.Incs.Flag) == 'Y')
        {
          if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
          {
            // ----------------------------------------------------------------
            // The AP for the current Case Unit was located at some time in
            // the past.  The "is Located" flag for the current Case Unit is
            // Y, which is correct, since the AP is still located through a
            // valid and current income source. Do not raise "Is Located"
            // events.
            // ----------------------------------------------------------------
            local.ApIsLocated.Flag = "Y";

            goto Test4;
          }

          // ----------------------------------------------------------------
          // The imported Case Unit "is located" flag is N, meaning the
          // AP is not currently located.
          // The AP has now been located via a verified income source.
          // Raise the appropriate event.
          // ----------------------------------------------------------------
          local.Infrastructure.ReasonCode = "INCSVRFD";
          local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case:";
            
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
            .Case1.Number;
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " located via income source on:";
            
          local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
            .TextWorkArea.Text8;
          local.ApIsLocated.Flag = "Y";
          local.CreateLocateInfra.Flag = "Y";

          break;
        }
      }
    }

Test4:

    // --------------------------------
    // Perform AP incarceration inquiry
    // --------------------------------
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      if (ReadIncarceration())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // ----------------------------------------------------------------
          // The AP for the current Case Unit was located at some time in
          // the past.  The "is Located" flag for the current Case Unit is
          // Y, which is correct, since the AP is still located through a
          // valid Incarceration. Do not raise "Is Located" events.
          // ----------------------------------------------------------------
          local.ApIsLocated.Flag = "Y";

          goto Test5;
        }

        // ----------------------------------------------------------------
        // The imported Case Unit "is located" flag is N, meaning the
        // AP is not currently located.
        // The AP has now been located via a verified incarceration.
        // Raise the appropriate event.
        // ----------------------------------------------------------------
        local.Infrastructure.ReasonCode = "APINCARC";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
          .Case1.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " located via incarc on:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test5:

    // -----------------------------------
    // Perform AP military service inquiry
    // -----------------------------------
    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      if (ReadMilitaryService())
      {
        if (AsChar(local.CaseUnitIsLocatedFlag.Text1) == 'Y')
        {
          // ----------------------------------------------------------------
          // The AP for the current Case Unit was located at some time in
          // the past.  The "is Located" flag for the current Case Unit is
          // Y, which is correct, since the AP is still located through a
          // current and valid Military Service. Do not raise "Is Located"
          // events.
          // ----------------------------------------------------------------
          local.ApIsLocated.Flag = "Y";

          goto Test6;
        }

        // The imported Case Unit "is located" flag is N, meaning the AP was not
        // located in the past.
        // The AP has now been located via a military service occurrence. Raise 
        // the appropriate event.
        local.Infrastructure.ReasonCode = "APINMIL";
        local.Infrastructure.Detail = "AP " + entities.ApCsePerson.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
          .Case1.Number;
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " located via military on:";
          
        local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
          .TextWorkArea.Text8;
        local.ApIsLocated.Flag = "Y";
        local.CreateLocateInfra.Flag = "Y";
      }
    }

Test6:

    if (AsChar(local.CreateLocateInfra.Flag) == 'Y')
    {
      export.ImportExportEvents.Index = export.ImportExportEvents.Count;
      export.ImportExportEvents.CheckSize();

      MoveInfrastructure(local.Infrastructure,
        export.ImportExportEvents.Update.G);
    }

    if (AsChar(local.ApIsLocated.Flag) == 'N')
    {
      local.Infrastructure.EventId = 10;
      local.Infrastructure.ReasonCode = "LOC_EXPIRED";
      local.Infrastructure.Detail = "AP not located on CU " + local
        .HoldCuNum.Text3;
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " on Case:";
        
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + entities
        .Case1.Number;
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + " as of:";
        
      local.Infrastructure.Detail = TrimEnd(local.Infrastructure.Detail) + local
        .TextWorkArea.Text8;

      export.ImportExportEvents.Index = export.ImportExportEvents.Count;
      export.ImportExportEvents.CheckSize();

      MoveInfrastructure(local.Infrastructure,
        export.ImportExportEvents.Update.G);
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.Current.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Infrastructure.CaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return Read("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetInt32(
          command, "cuNumber",
          import.Infrastructure.CaseUnitNumber.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.CasNo = db.GetString(reader, 2);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoChild = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAp ?? "");
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Ar.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoAr ?? "");
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.CaseUnit.Populated);
    entities.Ch.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.CaseUnit.CspNoChild ?? "");
      },
      (db, reader) =>
      {
        entities.Ch.Number = db.GetString(reader, 0);
        entities.Ch.Type1 = db.GetString(reader, 1);
        entities.Ch.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ch.Type1);
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.ApCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate1", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate2", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ApCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ApCsePersonAddress.Type1 = db.GetNullableString(reader, 2);
        entities.ApCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.ApCsePersonAddress.EndDate = db.GetNullableDate(reader, 4);
        entities.ApCsePersonAddress.LocationType = db.GetString(reader, 5);
        entities.ApCsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.ApCsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.ApCsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate1", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate2", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApCsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.ApCsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.ApCsePersonAddress.Type1 = db.GetNullableString(reader, 2);
        entities.ApCsePersonAddress.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.ApCsePersonAddress.EndDate = db.GetNullableDate(reader, 4);
        entities.ApCsePersonAddress.LocationType = db.GetString(reader, 5);
        entities.ApCsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.ApCsePersonAddress.LocationType);
      });
  }

  private bool ReadIncarceration()
  {
    entities.ApIncarceration.Populated = false;

    return Read("ReadIncarceration",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApIncarceration.CspNumber = db.GetString(reader, 0);
        entities.ApIncarceration.Identifier = db.GetInt32(reader, 1);
        entities.ApIncarceration.VerifiedDate = db.GetNullableDate(reader, 2);
        entities.ApIncarceration.EndDate = db.GetNullableDate(reader, 3);
        entities.ApIncarceration.StartDate = db.GetNullableDate(reader, 4);
        entities.ApIncarceration.Populated = true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.ApIncomeSource.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ApIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ApIncomeSource.ReturnCd = db.GetNullableString(reader, 2);
        entities.ApIncomeSource.CspINumber = db.GetString(reader, 3);
        entities.ApIncomeSource.StartDt = db.GetNullableDate(reader, 4);
        entities.ApIncomeSource.EndDt = db.GetNullableDate(reader, 5);
        entities.ApIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ApIncomeSource.Type1);

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadMilitaryService()
  {
    entities.ApMilitaryService.Populated = false;

    return Read("ReadMilitaryService",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ApMilitaryService.EffectiveDate = db.GetDate(reader, 0);
        entities.ApMilitaryService.CspNumber = db.GetString(reader, 1);
        entities.ApMilitaryService.StartDate = db.GetNullableDate(reader, 2);
        entities.ApMilitaryService.EndDate = db.GetNullableDate(reader, 3);
        entities.ApMilitaryService.Populated = true;
      });
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ImportExportEventsGroup group.</summary>
    [Serializable]
    public class ImportExportEventsGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public Infrastructure G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Infrastructure g;
    }

    /// <summary>
    /// Gets a value of ImportExportEvents.
    /// </summary>
    [JsonIgnore]
    public Array<ImportExportEventsGroup> ImportExportEvents =>
      importExportEvents ??= new(ImportExportEventsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ImportExportEvents for json serialization.
    /// </summary>
    [JsonPropertyName("importExportEvents")]
    [Computed]
    public IList<ImportExportEventsGroup> ImportExportEvents_Json
    {
      get => importExportEvents;
      set => ImportExportEvents.Assign(value);
    }

    private Array<ImportExportEventsGroup> importExportEvents;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Incs.
    /// </summary>
    [JsonPropertyName("incs")]
    public Common Incs
    {
      get => incs ??= new();
      set => incs = value;
    }

    /// <summary>
    /// A value of CreateLocateInfra.
    /// </summary>
    [JsonPropertyName("createLocateInfra")]
    public Common CreateLocateInfra
    {
      get => createLocateInfra ??= new();
      set => createLocateInfra = value;
    }

    /// <summary>
    /// A value of ApIsLocated.
    /// </summary>
    [JsonPropertyName("apIsLocated")]
    public Common ApIsLocated
    {
      get => apIsLocated ??= new();
      set => apIsLocated = value;
    }

    /// <summary>
    /// A value of CaseUnitIsLocatedFlag.
    /// </summary>
    [JsonPropertyName("caseUnitIsLocatedFlag")]
    public TextWorkArea CaseUnitIsLocatedFlag
    {
      get => caseUnitIsLocatedFlag ??= new();
      set => caseUnitIsLocatedFlag = value;
    }

    /// <summary>
    /// A value of HoldCuNum.
    /// </summary>
    [JsonPropertyName("holdCuNum")]
    public WorkArea HoldCuNum
    {
      get => holdCuNum ??= new();
      set => holdCuNum = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Common incs;
    private Common createLocateInfra;
    private Common apIsLocated;
    private TextWorkArea caseUnitIsLocatedFlag;
    private WorkArea holdCuNum;
    private TextWorkArea textWorkArea;
    private DateWorkArea null1;
    private DateWorkArea current;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ApMilitaryService.
    /// </summary>
    [JsonPropertyName("apMilitaryService")]
    public MilitaryService ApMilitaryService
    {
      get => apMilitaryService ??= new();
      set => apMilitaryService = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of ApIncarceration.
    /// </summary>
    [JsonPropertyName("apIncarceration")]
    public Incarceration ApIncarceration
    {
      get => apIncarceration ??= new();
      set => apIncarceration = value;
    }

    /// <summary>
    /// A value of ApIncomeSource.
    /// </summary>
    [JsonPropertyName("apIncomeSource")]
    public IncomeSource ApIncomeSource
    {
      get => apIncomeSource ??= new();
      set => apIncomeSource = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private MilitaryService apMilitaryService;
    private InterstateRequest interstateRequest;
    private Incarceration apIncarceration;
    private IncomeSource apIncomeSource;
    private CsePersonAddress apCsePersonAddress;
    private CsePerson ch;
    private CsePerson ar;
    private CsePerson apCsePerson;
    private CaseUnit caseUnit;
    private Case1 case1;
  }
#endregion
}
