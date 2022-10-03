// Program: SI_IIMC_CREATE_INCOMING_IS_CASE, ID: 372505525, model: 746.
// Short name: SWE02136
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
/// A program: SI_IIMC_CREATE_INCOMING_IS_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the Interstate Request which contains the basic information
/// for a referral for a specific Case and to a specific State.
/// </para>
/// </summary>
[Serializable]
public partial class SiIimcCreateIncomingIsCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_IIMC_CREATE_INCOMING_IS_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiIimcCreateIncomingIsCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiIimcCreateIncomingIsCase.
  /// </summary>
  public SiIimcCreateIncomingIsCase(IContext context, Import import,
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
    // ****************************************************************
    // 3/1/99  C Deghand    Per a request from the finance group, removed the 
    // code that automatically created person program.
    // 07/12/99 C Scroggins Commented out code which creates Interstate Request 
    // History record on Manual Closure at request of SME.
    // ****************************************************************
    // 04/09/01 swsrchf I00117417  Update Interstate Request, when it
    //                             already exits.
    //                             Removed commented out code.
    // ****************************************************************
    // 04/30/01 swsrchf I00118863  When Interstate Request already exists,
    //                             update ONLY, if it was created by an "LO1" or
    // "CSI".
    //                             Added the Other State case Id and Case Type 
    // attributes to
    //                             the UPDATE Interstate Request function.
    //                             Removed OLD commented out code.
    // ****************************************************************
    // 05/10/06 GVandy 	 WR230751	Add support for Tribal IV-D agencies.
    // 06/13/18   JHarden       CQ62215        Add a field for Fax # on IIMC
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    export.InterstateRequest.Assign(import.InterstateRequest);
    export.InterstateContact.Assign(import.InterstateContact);
    export.InterstateContactAddress.Assign(import.InterstateContactAddress);
    export.InterstatePaymentAddress.Assign(import.InterstatePaymentAddress);
    export.InterstateRequestHistory.Note = import.InterstateRequestHistory.Note;
    export.InterstateRequest.KsCaseInd = "N";

    if (Equal(export.InterstateRequest.OtherStateCaseClosureDate,
      local.Zero.Date))
    {
      export.InterstateRequest.OtherStateCaseClosureDate = local.Max.Date;
    }

    if (ReadCase())
    {
      if (AsChar(import.CaseMarkedDuplicate.Flag) == 'Y')
      {
        try
        {
          UpdateCase();

          // ------------------------------------------------------------
          // Create the History for manually marking a Duplicate Case.
          // ------------------------------------------------------------
          local.Infrastructure.InitiatingStateCode = "OS";
          local.Infrastructure.ProcessStatus = "Q";
          local.Infrastructure.EventId = 5;
          local.Infrastructure.ReasonCode = "MANUAL_DUP";
          local.Infrastructure.BusinessObjectCd = "CAS";
          local.Infrastructure.CaseNumber = import.Case1.Number;
          local.Infrastructure.UserId = "IIMC";
          local.Infrastructure.ReferenceDate = local.Current.Date;

          if (!IsEmpty(import.OtherState.StateAbbreviation))
          {
            local.Infrastructure.Detail =
              "Case manually marked as a Duplicate Case;" + " Initiating State :" +
              import.OtherState.StateAbbreviation;
          }
          else if (!IsEmpty(export.InterstateRequest.Country))
          {
            local.Infrastructure.Detail =
              "Case manually marked as a Duplicate Case;" + " Initiating Country :" +
              (export.InterstateRequest.Country ?? "");
          }
          else if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            local.Infrastructure.Detail =
              "Case manually marked as a Duplicate Case;" + " Initiating Tribal Agency :" +
              (export.InterstateRequest.TribalAgency ?? "");
          }

          foreach(var item in ReadCaseUnit())
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            UseSpCabCreateInfrastructure();
          }

          if (entities.CaseUnit.CuNumber == 0)
          {
            local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
            UseSpCabCreateInfrastructure();
          }

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
            IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_PV";

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
        try
        {
          UpdateCase();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "CASE_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "CASE_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      export.Case1.Assign(entities.Case1);
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!ReadAbsentParentCsePerson())
    {
      ExitState = "NO_APS_ON_A_CASE";

      return;
    }

    // ------------------------------------------------------------
    // Check if an Interstate Case already exists. If not, create it.
    // ------------------------------------------------------------
    if (ReadInterstateCase())
    {
      export.InterstateCase.Assign(entities.InterstateCase);
    }
    else
    {
      try
      {
        CreateInterstateCase();
        export.InterstateCase.Assign(entities.InterstateCase);
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

    // ------------------------------------------------------------
    // Check if an Interstate Request already exists
    // for the the Case and the AP from that State.
    // ------------------------------------------------------------
    if (ReadInterstateRequest2())
    {
      // ****************************************************************
      // 04/01/2002 T.Bobb PR00129803
      // Do not allow a second manual conversion. If manual conversion already 
      // exists, user should us reopen instead of add.
      // ****************************************************************
      if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'C' && (
        AsChar(entities.InterstateRequest.KsCaseInd) == 'N' || AsChar
        (entities.InterstateRequest.KsCaseInd) == 'Y'))
      {
        if (ReadInterstateRequestHistory1())
        {
          ExitState = "ACO_NE00000_MANUAL_CONV_AE";

          return;
        }
      }

      // *** Problem report I00117417
      // *** 04/09/01 swsrchf
      // *** start
      export.InterstateRequest.Assign(entities.InterstateRequest);

      // *** Problem report I00118863
      // *** 04/30/01 swsrchf
      // *** start
      local.Found.Flag = "Y";

      foreach(var item in ReadInterstateRequestHistory2())
      {
        if (Equal(entities.InterstateRequestHistory.FunctionalTypeCode, "LO1") ||
          Equal(entities.InterstateRequestHistory.FunctionalTypeCode, "CSI"))
        {
          local.Found.Flag = "Y";

          break;
        }

        if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'C')
        {
        }
        else
        {
          if (AsChar(export.InterstateRequest.KsCaseInd) == 'Y' && AsChar
            (import.InterstateRequest.OtherStateCaseStatus) == 'O')
          {
            ExitState = "ACO_NE0000_OPEN_OUT_INTER_REQ_AE";
          }
          else
          {
            ExitState = "INTERSTATE_CASE_AE_FOR_THE_COMBN";
          }

          return;
        }
      }

      // *** end
      // *** 04/30/01 swsrchf
      // *** Problem report I00118863
    }
    else
    {
      local.Found.Flag = "N";
    }

    if (AsChar(local.Found.Flag) == 'Y')
    {
      // >>
      // WR-10501 Do not allow an add or update if an outgoing
      // open Interstate request exits for the same state.
      // You must close the outgoing request first.
      // Tom Bobb (swdptmb)  6/8/01
      if (AsChar(entities.InterstateRequest.OtherStateCaseStatus) == 'O' && AsChar
        (entities.InterstateRequest.KsCaseInd) == 'Y')
      {
        ExitState = "ACO_NE0000_OPEN_OUT_INTER_REQ_AE";

        return;

        // *** end
        // *** 06/08/01  Tom Bobb (swdptmb)
        // *** WR - 10501
      }

      // *** Problem report I00118863
      // *** 04/30/01 swsrchf
      // *** Added Other State Case Id and Case Type attributes to the UPDATE 
      // statement
      try
      {
        UpdateInterstateRequest();
        export.InterstateRequest.KsCaseInd =
          entities.InterstateRequest.KsCaseInd;
        export.InterstateRequest.OtherStateCaseStatus =
          entities.InterstateRequest.OtherStateCaseStatus;
        export.InterstateRequest.OtherStateCaseClosureDate =
          entities.InterstateRequest.OtherStateCaseClosureDate;
        export.InterstateRequest.OtherStateCaseClosureReason =
          entities.InterstateRequest.OtherStateCaseClosureReason;
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTATE_REQUEST_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_INTERSTAT_REQUEST_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
      {
        return;
      }
    }
    else
    {
      // *** end
      // *** 04/09/01 swsrchf
      // *** Problem report I00117417
      // ------------------------------------------------------------
      // Create the Interstate Request for the Case and the AP.
      // ------------------------------------------------------------
      ReadInterstateRequest3();

      // 9/25/02 Tom Bobb set other_case_status to 'O".
      export.InterstateRequest.IntHGeneratedId =
        entities.InterstateRequest.IntHGeneratedId + 1;

      try
      {
        CreateInterstateRequest();
        AssociateInterstateRequest1();
        AssociateInterstateRequest2();
        export.InterstateRequest.Assign(entities.InterstateRequest);
        export.IreqCreated.Date =
          Date(entities.InterstateRequest.CreatedTimestamp);
        export.IreqUpdated.Date =
          Date(entities.InterstateRequest.LastUpdatedTimestamp);
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_INTERSTAT_REQUEST_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_INTERSTAT_REQUEST_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        return;
      }
    }

    // ------------------------------------------------------------
    // Create the Interstate Request Contact and Address.
    // ------------------------------------------------------------
    // >>
    // 2/8/2001 T.Bobb added logic that if contact info already
    // exits, update it.
    export.InterstateContactAddress.Type1 = "CT";

    if (!IsEmpty(export.InterstateContactAddress.State))
    {
      export.InterstateContactAddress.LocationType = "D";
    }
    else if (!IsEmpty(export.InterstateContactAddress.Country))
    {
      export.InterstateContactAddress.LocationType = "F";
    }

    // >>
    // If contact exists, delete it and re-add it
    if (ReadInterstateContact())
    {
      DeleteInterstateContact();
    }

    try
    {
      CreateInterstateContact();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

      try
      {
        CreateInterstateContactAddress();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "SI0000_IS_CONTACT_ADDR_ADD_ERROR";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "SI0000_IS_CONTACT_ADDR_ADD_ERROR";

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
          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTAT_CONTACT_ADD_ERR";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      return;
    }

    // ------------------------------------------------------------
    // Create the Interstate Request Payment Address.
    // ------------------------------------------------------------
    export.InterstatePaymentAddress.Type1 = "PY";

    if (!IsEmpty(export.InterstatePaymentAddress.State))
    {
      export.InterstatePaymentAddress.LocationType = "D";
    }
    else if (!IsEmpty(export.InterstatePaymentAddress.Country))
    {
      export.InterstatePaymentAddress.LocationType = "F";
    }

    if (ReadInterstatePaymentAddress())
    {
      DeleteInterstatePaymentAddress();
    }

    try
    {
      CreateInterstatePaymentAddress();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "INTERSTATE_CASE_AE_FOR_THE_COMBN";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "SI0000_INTERSTAT_PAYMENT_ADD_ERR";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      return;
    }

    // ------------------------------------------------------------
    // Create the Interstate Request History for manual conversion.
    // ------------------------------------------------------------
    export.InterstateRequestHistory.ActionReasonCode = "IICNV";
    export.InterstateRequestHistory.ActionCode = "";
    export.InterstateRequestHistory.FunctionalTypeCode = "";
    export.InterstateRequestHistory.TransactionDirectionInd = "";
    export.InterstateRequestHistory.AttachmentIndicator = "";
    export.InterstateRequestHistory.TransactionSerialNum = 0;
    export.InterstateRequestHistory.ActionResolutionDate = local.Zero.Date;
    export.InterstateRequestHistory.TransactionDate = local.Current.Date;
    export.InterstateRequestHistory.CreatedBy = "SWEIIIMC";
    UseSiCreateIsRequestHistory();

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
      IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------
    // Create the History for manual Interstate conversion.
    // ------------------------------------------------------------
    local.Infrastructure.InitiatingStateCode = "OS";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.EventId = 5;
    local.Infrastructure.ReasonCode = "IN_INTST_MCONV";
    local.Infrastructure.BusinessObjectCd = "CAS";
    local.Infrastructure.CaseNumber = import.Case1.Number;
    local.Infrastructure.UserId = "IIMC";
    local.Infrastructure.ReferenceDate = local.Current.Date;

    foreach(var item in ReadCaseUnit())
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

      if (!IsEmpty(import.OtherState.StateAbbreviation))
      {
        local.Infrastructure.Detail =
          "Incoming Interstate Case Converted Manually;" + " Initiating State :" +
          import.OtherState.StateAbbreviation;
      }
      else if (!IsEmpty(export.InterstateRequest.Country))
      {
        local.Infrastructure.Detail =
          "Incoming Interstate Case Converted Manually;" + " Initiating Country :" +
          (export.InterstateRequest.Country ?? "");
      }
      else if (!IsEmpty(export.InterstateRequest.TribalAgency))
      {
        local.Infrastructure.Detail =
          "Incoming Interstate Case Converted Manually;" + " Initiating Tribal Agency :" +
          (export.InterstateRequest.TribalAgency ?? "");
      }

      UseSpCabCreateInfrastructure();
    }

    if (entities.CaseUnit.CuNumber == 0)
    {
      local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
      local.Infrastructure.ReasonCode = "IN_INTST_MCVNAP";

      if (!IsEmpty(export.InterstateRequest.TribalAgency))
      {
        local.Infrastructure.Detail =
          "Incoming Interstate Case (no Case Units) Converted Manually;" + " IN Tribal Agency :" +
          (export.InterstateRequest.TribalAgency ?? "");
      }
      else if (!IsEmpty(import.OtherState.StateAbbreviation))
      {
        local.Infrastructure.Detail =
          "Incoming Interstate Case (no Case Units) Converted Manually;" + " IN State :" +
          import.OtherState.StateAbbreviation;
      }
      else if (!IsEmpty(export.InterstateRequest.Country))
      {
        local.Infrastructure.Detail =
          "Incoming Interstate Case (no Case Units) Converted Manually;" + " IN Country :" +
          (export.InterstateRequest.Country ?? "");
      }

      UseSpCabCreateInfrastructure();
    }

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
      IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (AsChar(export.InterstateRequest.OtherStateCaseStatus) == 'C')
    {
      export.InterstateRequestHistory.ActionReasonCode = "IICLS";
      export.InterstateRequestHistory.ActionCode = "";
      export.InterstateRequestHistory.FunctionalTypeCode = "";
      export.InterstateRequestHistory.TransactionDirectionInd = "";
      export.InterstateRequestHistory.AttachmentIndicator = "";
      export.InterstateRequestHistory.Note =
        Spaces(InterstateRequestHistory.Note_MaxLength);
      export.InterstateRequestHistory.TransactionSerialNum = 0;
      export.InterstateRequestHistory.ActionResolutionDate = local.Zero.Date;
      export.InterstateRequestHistory.TransactionDate = local.Current.Date;

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ------------------------------------------------------------
      // Create the History for manual Interstate Case closure.
      // ------------------------------------------------------------
      local.Infrastructure.InitiatingStateCode = "OS";
      local.Infrastructure.ProcessStatus = "Q";
      local.Infrastructure.EventId = 5;
      local.Infrastructure.ReasonCode = "IN_INTST_MCLOSE";
      local.Infrastructure.BusinessObjectCd = "CAS";
      local.Infrastructure.CaseNumber = import.Case1.Number;
      local.Infrastructure.UserId = "IIMC";
      local.Infrastructure.ReferenceDate = local.Current.Date;

      foreach(var item in ReadCaseUnit())
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;

        if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          local.Infrastructure.Detail =
            "Incoming Interstate Case Closed Manually;" + " Initiating Tribal Agency :" +
            (export.InterstateRequest.TribalAgency ?? "");
        }
        else if (!IsEmpty(import.OtherState.StateAbbreviation))
        {
          local.Infrastructure.Detail =
            "Incoming Interstate Case Closed Manually;" + " Initiating State :" +
            import.OtherState.StateAbbreviation;
        }
        else if (!IsEmpty(export.InterstateRequest.Country))
        {
          local.Infrastructure.Detail =
            "Incoming Interstate Case Closed Manually;" + " Initiating Country :" +
            (export.InterstateRequest.Country ?? "");
        }

        UseSpCabCreateInfrastructure();
      }

      if (entities.CaseUnit.CuNumber == 0)
      {
        local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
        local.Infrastructure.ReasonCode = "IN_INTST_MCLNAP";

        if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          local.Infrastructure.Detail =
            "Incoming Interstate Case (no Case Units) Closed Manually;" + " IN Tribal Agency :" +
            (export.InterstateRequest.TribalAgency ?? "");
        }
        else if (!IsEmpty(import.OtherState.StateAbbreviation))
        {
          local.Infrastructure.Detail =
            "Incoming Interstate Case (no Case Units) Closed Manually;" + " IN State :" +
            import.OtherState.StateAbbreviation;
        }
        else if (!IsEmpty(export.InterstateRequest.Country))
        {
          local.Infrastructure.Detail =
            "Incoming Interstate Case (no Case Units) Closed Manually;" + " IN Country :" +
            (export.InterstateRequest.Country ?? "");
        }

        UseSpCabCreateInfrastructure();
      }

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // ------------------------------------------------------------
      // Check to see if there are any more Interstate Case open
      // for a Duplicate Case.
      // ------------------------------------------------------------
      if (AsChar(entities.Case1.DuplicateCaseIndicator) == 'Y')
      {
        if (!ReadInterstateRequest1())
        {
          // ------------------------------------------------------------
          // No more Interstate Case open for a Duplicate Case. Change
          // the Duplicate indicator flag to "N" and write to History.
          // ------------------------------------------------------------
          try
          {
            UpdateCase();

            // ------------------------------------------------------------
            // Create the History for marking a Case as not duplicate.
            // ------------------------------------------------------------
            local.Infrastructure.InitiatingStateCode = "OS";
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.EventId = 5;
            local.Infrastructure.ReasonCode = "IS_CASE_CLS_NOD";
            local.Infrastructure.BusinessObjectCd = "CAS";
            local.Infrastructure.CaseNumber = import.Case1.Number;
            local.Infrastructure.UserId = "IIMC";
            local.Infrastructure.ReferenceDate = local.Current.Date;
            local.Infrastructure.Detail =
              "Case no longer marked Duplicate; Last Interstate Case involvement closed";
              

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
              UseSpCabCreateInfrastructure();
            }

            if (entities.CaseUnit.CuNumber == 0)
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
              UseSpCabCreateInfrastructure();
            }

            if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
              IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.Case1.Assign(entities.Case1);
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "CASE_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "CASE_PV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }

    // ------------------------------------------------------------
    // Create the Person Programs for the Interstate Case.
    // ------------------------------------------------------------
    foreach(var item in ReadChildCsePerson())
    {
      local.InterstateChild.Number = entities.Child2.Number;

      // ------------------------------------------------------------
      // Create the Interstate Roles for the Children in the Case.
      // ------------------------------------------------------------
      UseSiCreateInterstateRole();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD") && !
        IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInterstateRequest(InterstateRequest source,
    InterstateRequest target)
  {
    target.IntHGeneratedId = source.IntHGeneratedId;
    target.OtherStateFips = source.OtherStateFips;
    target.Country = source.Country;
    target.TribalAgency = source.TribalAgency;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiCreateInterstateRole()
  {
    var useImport = new SiCreateInterstateRole.Import();
    var useExport = new SiCreateInterstateRole.Export();

    useImport.Child.Number = entities.Child2.Number;
    useImport.InterstateRequest.Assign(entities.InterstateRequest);

    Call(SiCreateInterstateRole.Execute, useImport, useExport);
  }

  private void UseSiCreateIsRequestHistory()
  {
    var useImport = new SiCreateIsRequestHistory.Import();
    var useExport = new SiCreateIsRequestHistory.Export();

    useImport.Case1.Number = entities.Case1.Number;
    useImport.Ap.Number = entities.Ap.Number;
    useImport.InterstateRequestHistory.Assign(export.InterstateRequestHistory);
    MoveInterstateRequest(export.InterstateRequest, useImport.InterstateRequest);
      

    Call(SiCreateIsRequestHistory.Execute, useImport, useExport);

    export.InterstateRequestHistory.Assign(useExport.InterstateRequestHistory);
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void AssociateInterstateRequest1()
  {
    var casINumber = entities.Case1.Number;

    entities.InterstateRequest.Populated = false;
    Update("AssociateInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", casINumber);
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.CasINumber = casINumber;
    entities.InterstateRequest.Populated = true;
  }

  private void AssociateInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);

    var casNumber = entities.AbsentParent.CasNumber;
    var cspNumber = entities.AbsentParent.CspNumber;
    var croType = entities.AbsentParent.Type1;
    var croId = entities.AbsentParent.Identifier;

    CheckValid<InterstateRequest>("CroType", croType);
    entities.InterstateRequest.Populated = false;
    Update("AssociateInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", casNumber);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "croType", croType);
        db.SetNullableInt32(command, "croId", croId);
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.CasNumber = casNumber;
    entities.InterstateRequest.CspNumber = cspNumber;
    entities.InterstateRequest.CroType = croType;
    entities.InterstateRequest.CroId = croId;
    entities.InterstateRequest.Populated = true;
  }

  private void CreateInterstateCase()
  {
    var transSerialNumber = entities.Case1.IcTransSerialNumber;
    var transactionDate = entities.Case1.IcTransactionDate;

    entities.InterstateCase.Populated = false;
    Update("CreateInterstateCase",
      (db, command) =>
      {
        db.SetInt32(command, "localFipsState", 0);
        db.SetNullableInt32(command, "localFipsCounty", 0);
        db.SetInt32(command, "otherFipsState", 0);
        db.SetInt64(command, "transSerialNbr", transSerialNumber);
        db.SetString(command, "actionCode", "");
        db.SetString(command, "functionalTypeCo", "");
        db.SetDate(command, "transactionDate", transactionDate);
        db.SetNullableString(command, "ksCaseId", "");
        db.SetNullableString(command, "actionReasonCode", "");
        db.SetNullableDate(command, "actionResolution", default(DateTime));
        db.SetNullableInt32(command, "caseDataInd", 0);
        db.SetNullableTimeSpan(command, "sentTime", TimeSpan.Zero);
        db.SetNullableString(command, "paymentMailingAd", "");
        db.SetNullableString(command, "paymentState", "");
        db.SetNullableString(command, "paymentZipCode4", "");
        db.SetNullableString(command, "contactNameLast", "");
        db.SetNullableString(command, "contactNameFirst", "");
        db.SetNullableInt32(command, "contactPhoneNum", 0);
        db.SetNullableString(command, "memo", "");
        db.SetNullableString(command, "contactPhoneExt", "");
        db.SetNullableString(command, "conInternetAddr", "");
        db.SetNullableString(command, "sendPaymBankAcc", "");
        db.SetNullableInt32(command, "sendPaymRtCode", 0);
      });

    entities.InterstateCase.OtherFipsState = 0;
    entities.InterstateCase.TransSerialNumber = transSerialNumber;
    entities.InterstateCase.TransactionDate = transactionDate;
    entities.InterstateCase.SendPaymentsBankAccount = "";
    entities.InterstateCase.SendPaymentsRoutingCode = 0;
    entities.InterstateCase.Populated = true;
  }

  private void CreateInterstateContact()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var startDate = local.Current.Date;
    var contactPhoneNum =
      export.InterstateContact.ContactPhoneNum.GetValueOrDefault();
    var endDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var nameLast = export.InterstateContact.NameLast ?? "";
    var nameFirst = export.InterstateContact.NameFirst ?? "";
    var nameMiddle = export.InterstateContact.NameMiddle ?? "";
    var contactNameSuffix = export.InterstateContact.ContactNameSuffix ?? "";
    var areaCode = export.InterstateContact.AreaCode.GetValueOrDefault();
    var contactPhoneExtension =
      export.InterstateContact.ContactPhoneExtension ?? "";
    var contactFaxNumber =
      export.InterstateContact.ContactFaxNumber.GetValueOrDefault();
    var contactFaxAreaCode =
      export.InterstateContact.ContactFaxAreaCode.GetValueOrDefault();
    var contactInternetAddress =
      export.InterstateContact.ContactInternetAddress ?? "";

    entities.InterstateContact.Populated = false;
    Update("CreateInterstateContact",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "startDate", startDate);
        db.SetNullableInt32(command, "contactPhoneNum", contactPhoneNum);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "nameLast", nameLast);
        db.SetNullableString(command, "nameFirst", nameFirst);
        db.SetNullableString(command, "nameMiddle", nameMiddle);
        db.SetNullableString(command, "contactNameSuffi", contactNameSuffix);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableString(command, "contactPhoneExt", contactPhoneExtension);
        db.SetNullableInt32(command, "contactFaxNumber", contactFaxNumber);
        db.SetNullableInt32(command, "contFaxAreaCode", contactFaxAreaCode);
        db.
          SetNullableString(command, "contInternetAddr", contactInternetAddress);
          
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTimes", default(DateTime));
      });

    entities.InterstateContact.IntGeneratedId = intGeneratedId;
    entities.InterstateContact.StartDate = startDate;
    entities.InterstateContact.ContactPhoneNum = contactPhoneNum;
    entities.InterstateContact.EndDate = endDate;
    entities.InterstateContact.CreatedBy = createdBy;
    entities.InterstateContact.CreatedTstamp = createdTstamp;
    entities.InterstateContact.NameLast = nameLast;
    entities.InterstateContact.NameFirst = nameFirst;
    entities.InterstateContact.NameMiddle = nameMiddle;
    entities.InterstateContact.ContactNameSuffix = contactNameSuffix;
    entities.InterstateContact.AreaCode = areaCode;
    entities.InterstateContact.ContactPhoneExtension = contactPhoneExtension;
    entities.InterstateContact.ContactFaxNumber = contactFaxNumber;
    entities.InterstateContact.ContactFaxAreaCode = contactFaxAreaCode;
    entities.InterstateContact.ContactInternetAddress = contactInternetAddress;
    entities.InterstateContact.Populated = true;
  }

  private void CreateInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);

    var icoContStartDt = entities.InterstateContact.StartDate;
    var intGeneratedId = entities.InterstateContact.IntGeneratedId;
    var startDate = local.Current.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = export.InterstateContactAddress.Type1 ?? "";
    var street1 = export.InterstateContactAddress.Street1 ?? "";
    var street2 = export.InterstateContactAddress.Street2 ?? "";
    var city = export.InterstateContactAddress.City ?? "";
    var endDate = local.Max.Date;
    var state = export.InterstateContactAddress.State ?? "";
    var zipCode = export.InterstateContactAddress.ZipCode ?? "";
    var zip4 = export.InterstateContactAddress.Zip4 ?? "";
    var zip3 = export.InterstateContactAddress.Zip3 ?? "";
    var street3 = export.InterstateContactAddress.Street3 ?? "";
    var street4 = export.InterstateContactAddress.Street4 ?? "";
    var province = export.InterstateContactAddress.Province ?? "";
    var postalCode = export.InterstateContactAddress.PostalCode ?? "";
    var country = export.InterstateContactAddress.Country ?? "";
    var locationType = export.InterstateContactAddress.LocationType;

    CheckValid<InterstateContactAddress>("LocationType", locationType);
    entities.InterstateContactAddress.Populated = false;
    Update("CreateInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(command, "icoContStartDt", icoContStartDt);
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "startDate", startDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "county", "");
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
      });

    entities.InterstateContactAddress.IcoContStartDt = icoContStartDt;
    entities.InterstateContactAddress.IntGeneratedId = intGeneratedId;
    entities.InterstateContactAddress.StartDate = startDate;
    entities.InterstateContactAddress.CreatedBy = createdBy;
    entities.InterstateContactAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstateContactAddress.LastUpdatedBy = createdBy;
    entities.InterstateContactAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstateContactAddress.Type1 = type1;
    entities.InterstateContactAddress.Street1 = street1;
    entities.InterstateContactAddress.Street2 = street2;
    entities.InterstateContactAddress.City = city;
    entities.InterstateContactAddress.EndDate = endDate;
    entities.InterstateContactAddress.State = state;
    entities.InterstateContactAddress.ZipCode = zipCode;
    entities.InterstateContactAddress.Zip4 = zip4;
    entities.InterstateContactAddress.Zip3 = zip3;
    entities.InterstateContactAddress.Street3 = street3;
    entities.InterstateContactAddress.Street4 = street4;
    entities.InterstateContactAddress.Province = province;
    entities.InterstateContactAddress.PostalCode = postalCode;
    entities.InterstateContactAddress.Country = country;
    entities.InterstateContactAddress.LocationType = locationType;
    entities.InterstateContactAddress.Populated = true;
  }

  private void CreateInterstatePaymentAddress()
  {
    var intGeneratedId = entities.InterstateRequest.IntHGeneratedId;
    var addressStartDate = local.Current.Date;
    var type1 = export.InterstatePaymentAddress.Type1 ?? "";
    var street1 = export.InterstatePaymentAddress.Street1;
    var street2 = export.InterstatePaymentAddress.Street2 ?? "";
    var city = export.InterstatePaymentAddress.City;
    var addressEndDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var payableToName = export.InterstatePaymentAddress.PayableToName ?? "";
    var state = export.InterstatePaymentAddress.State ?? "";
    var zipCode = export.InterstatePaymentAddress.ZipCode ?? "";
    var zip4 = export.InterstatePaymentAddress.Zip4 ?? "";
    var zip3 = export.InterstatePaymentAddress.Zip3 ?? "";
    var street3 = export.InterstatePaymentAddress.Street3 ?? "";
    var street4 = export.InterstatePaymentAddress.Street4 ?? "";
    var province = export.InterstatePaymentAddress.Province ?? "";
    var postalCode = export.InterstatePaymentAddress.PostalCode ?? "";
    var country = export.InterstatePaymentAddress.Country ?? "";
    var locationType = export.InterstatePaymentAddress.LocationType;
    var fipsCounty = export.InterstatePaymentAddress.FipsCounty ?? "";
    var fipsState = NumberToString(export.InterstateCase.OtherFipsState, 2);
    var fipsLocation = export.InterstatePaymentAddress.FipsLocation ?? "";

    CheckValid<InterstatePaymentAddress>("LocationType", locationType);
    entities.InterstatePaymentAddress.Populated = false;
    Update("CreateInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "addressStartDate", addressStartDate);
        db.SetNullableString(command, "type", type1);
        db.SetString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetString(command, "city", city);
        db.SetNullableString(command, "zip5", "");
        db.SetNullableDate(command, "addressEndDate", addressEndDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "payableToName", payableToName);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", "");
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "fipsCounty", fipsCounty);
        db.SetNullableString(command, "fipsState", fipsState);
        db.SetNullableString(command, "fipsLocation", fipsLocation);
        db.SetNullableInt32(command, "routingNumberAba", 0);
        db.SetNullableString(command, "accountNumberDfi", "");
        db.SetNullableString(command, "accountType", "");
      });

    entities.InterstatePaymentAddress.IntGeneratedId = intGeneratedId;
    entities.InterstatePaymentAddress.AddressStartDate = addressStartDate;
    entities.InterstatePaymentAddress.Type1 = type1;
    entities.InterstatePaymentAddress.Street1 = street1;
    entities.InterstatePaymentAddress.Street2 = street2;
    entities.InterstatePaymentAddress.City = city;
    entities.InterstatePaymentAddress.AddressEndDate = addressEndDate;
    entities.InterstatePaymentAddress.CreatedBy = createdBy;
    entities.InterstatePaymentAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.LastUpdatedBy = createdBy;
    entities.InterstatePaymentAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstatePaymentAddress.PayableToName = payableToName;
    entities.InterstatePaymentAddress.State = state;
    entities.InterstatePaymentAddress.ZipCode = zipCode;
    entities.InterstatePaymentAddress.Zip4 = zip4;
    entities.InterstatePaymentAddress.Zip3 = zip3;
    entities.InterstatePaymentAddress.Street3 = street3;
    entities.InterstatePaymentAddress.Street4 = street4;
    entities.InterstatePaymentAddress.Province = province;
    entities.InterstatePaymentAddress.PostalCode = postalCode;
    entities.InterstatePaymentAddress.Country = country;
    entities.InterstatePaymentAddress.LocationType = locationType;
    entities.InterstatePaymentAddress.FipsCounty = fipsCounty;
    entities.InterstatePaymentAddress.FipsState = fipsState;
    entities.InterstatePaymentAddress.FipsLocation = fipsLocation;
    entities.InterstatePaymentAddress.RoutingNumberAba = 0;
    entities.InterstatePaymentAddress.AccountNumberDfi = "";
    entities.InterstatePaymentAddress.AccountType = "";
    entities.InterstatePaymentAddress.Populated = true;
  }

  private void CreateInterstateRequest()
  {
    var intHGeneratedId = export.InterstateRequest.IntHGeneratedId;
    var otherStateCaseId = export.InterstateRequest.OtherStateCaseId ?? "";
    var otherStateFips = export.InterstateRequest.OtherStateFips;
    var createdBy = "SWEIIIMC";
    var createdTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var otherStateCaseStatus = "O";
    var caseType = export.InterstateRequest.CaseType ?? "";
    var ksCaseInd = export.InterstateRequest.KsCaseInd;
    var otherStateCaseClosureReason =
      export.InterstateRequest.OtherStateCaseClosureReason ?? "";
    var otherStateCaseClosureDate = local.Current.Date;
    var country = export.InterstateRequest.Country ?? "";
    var tribalAgency = export.InterstateRequest.TribalAgency ?? "";

    entities.InterstateRequest.Populated = false;
    Update("CreateInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", intHGeneratedId);
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetInt32(command, "othrStateFipsCd", otherStateFips);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetString(command, "ksCaseInd", ksCaseInd);
        db.SetNullableString(
          command, "othStateClsRes", otherStateCaseClosureReason);
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "tribalAgency", tribalAgency);
      });

    entities.InterstateRequest.IntHGeneratedId = intHGeneratedId;
    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.OtherStateFips = otherStateFips;
    entities.InterstateRequest.CreatedBy = createdBy;
    entities.InterstateRequest.CreatedTimestamp = createdTimestamp;
    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
    entities.InterstateRequest.KsCaseInd = ksCaseInd;
    entities.InterstateRequest.OtherStateCaseClosureReason =
      otherStateCaseClosureReason;
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.CasINumber = null;
    entities.InterstateRequest.CasNumber = null;
    entities.InterstateRequest.CspNumber = null;
    entities.InterstateRequest.CroType = null;
    entities.InterstateRequest.CroId = null;
    entities.InterstateRequest.Country = country;
    entities.InterstateRequest.TribalAgency = tribalAgency;
    entities.InterstateRequest.Populated = true;
  }

  private void DeleteInterstateContact()
  {
    Update("DeleteInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.SetDate(
          command, "startDate",
          entities.InterstateContact.StartDate.GetValueOrDefault());
      });
  }

  private void DeleteInterstatePaymentAddress()
  {
    Update("DeleteInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstatePaymentAddress.IntGeneratedId);
        db.SetDate(
          command, "addressStartDate",
          entities.InterstatePaymentAddress.AddressStartDate.
            GetValueOrDefault());
      });
  }

  private bool ReadAbsentParentCsePerson()
  {
    entities.AbsentParent.Populated = false;
    entities.Ap.Populated = false;

    return Read("ReadAbsentParentCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.AbsentParent.CasNumber = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 1);
        entities.Ap.Number = db.GetString(reader, 1);
        entities.AbsentParent.Type1 = db.GetString(reader, 2);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.Populated = true;
        entities.Ap.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 1);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 2);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.Case1.IcTransSerialNumber = db.GetInt64(reader, 4);
        entities.Case1.IcTransactionDate = db.GetDate(reader, 5);
        entities.Case1.DuplicateCaseIndicator = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.State = db.GetString(reader, 1);
        entities.CaseUnit.StartDate = db.GetDate(reader, 2);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 3);
        entities.CaseUnit.ClosureReasonCode = db.GetNullableString(reader, 4);
        entities.CaseUnit.CasNo = db.GetString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadChildCsePerson()
  {
    entities.Child1.Populated = false;
    entities.Child2.Populated = false;

    return ReadEach("ReadChildCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Child1.CasNumber = db.GetString(reader, 0);
        entities.Child1.CspNumber = db.GetString(reader, 1);
        entities.Child2.Number = db.GetString(reader, 1);
        entities.Child1.Type1 = db.GetString(reader, 2);
        entities.Child1.Identifier = db.GetInt32(reader, 3);
        entities.Child1.StartDate = db.GetNullableDate(reader, 4);
        entities.Child1.EndDate = db.GetNullableDate(reader, 5);
        entities.Child1.Populated = true;
        entities.Child2.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child1.Type1);

        return true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", entities.Case1.IcTransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          entities.Case1.IcTransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.OtherFipsState = db.GetInt32(reader, 0);
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 1);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 2);
        entities.InterstateCase.SendPaymentsBankAccount =
          db.GetNullableString(reader, 3);
        entities.InterstateCase.SendPaymentsRoutingCode =
          db.GetNullableInt64(reader, 4);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.ContactPhoneNum =
          db.GetNullableInt32(reader, 2);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 3);
        entities.InterstateContact.CreatedBy = db.GetString(reader, 4);
        entities.InterstateContact.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.InterstateContact.NameLast = db.GetNullableString(reader, 6);
        entities.InterstateContact.NameFirst = db.GetNullableString(reader, 7);
        entities.InterstateContact.NameMiddle = db.GetNullableString(reader, 8);
        entities.InterstateContact.ContactNameSuffix =
          db.GetNullableString(reader, 9);
        entities.InterstateContact.AreaCode = db.GetNullableInt32(reader, 10);
        entities.InterstateContact.ContactPhoneExtension =
          db.GetNullableString(reader, 11);
        entities.InterstateContact.ContactFaxNumber =
          db.GetNullableInt32(reader, 12);
        entities.InterstateContact.ContactFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.InterstateContact.ContactInternetAddress =
          db.GetNullableString(reader, 14);
        entities.InterstateContact.Populated = true;
      });
  }

  private bool ReadInterstatePaymentAddress()
  {
    entities.InterstatePaymentAddress.Populated = false;

    return Read("ReadInterstatePaymentAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstatePaymentAddress.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstatePaymentAddress.AddressStartDate =
          db.GetDate(reader, 1);
        entities.InterstatePaymentAddress.Type1 =
          db.GetNullableString(reader, 2);
        entities.InterstatePaymentAddress.Street1 = db.GetString(reader, 3);
        entities.InterstatePaymentAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.InterstatePaymentAddress.City = db.GetString(reader, 5);
        entities.InterstatePaymentAddress.AddressEndDate =
          db.GetNullableDate(reader, 6);
        entities.InterstatePaymentAddress.CreatedBy = db.GetString(reader, 7);
        entities.InterstatePaymentAddress.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.InterstatePaymentAddress.LastUpdatedBy =
          db.GetString(reader, 9);
        entities.InterstatePaymentAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.InterstatePaymentAddress.PayableToName =
          db.GetNullableString(reader, 11);
        entities.InterstatePaymentAddress.State =
          db.GetNullableString(reader, 12);
        entities.InterstatePaymentAddress.ZipCode =
          db.GetNullableString(reader, 13);
        entities.InterstatePaymentAddress.Zip4 =
          db.GetNullableString(reader, 14);
        entities.InterstatePaymentAddress.Zip3 =
          db.GetNullableString(reader, 15);
        entities.InterstatePaymentAddress.Street3 =
          db.GetNullableString(reader, 16);
        entities.InterstatePaymentAddress.Street4 =
          db.GetNullableString(reader, 17);
        entities.InterstatePaymentAddress.Province =
          db.GetNullableString(reader, 18);
        entities.InterstatePaymentAddress.PostalCode =
          db.GetNullableString(reader, 19);
        entities.InterstatePaymentAddress.Country =
          db.GetNullableString(reader, 20);
        entities.InterstatePaymentAddress.LocationType =
          db.GetString(reader, 21);
        entities.InterstatePaymentAddress.FipsCounty =
          db.GetNullableString(reader, 22);
        entities.InterstatePaymentAddress.FipsState =
          db.GetNullableString(reader, 23);
        entities.InterstatePaymentAddress.FipsLocation =
          db.GetNullableString(reader, 24);
        entities.InterstatePaymentAddress.RoutingNumberAba =
          db.GetNullableInt64(reader, 25);
        entities.InterstatePaymentAddress.AccountNumberDfi =
          db.GetNullableString(reader, 26);
        entities.InterstatePaymentAddress.AccountType =
          db.GetNullableString(reader, 27);
        entities.InterstatePaymentAddress.Populated = true;
        CheckValid<InterstatePaymentAddress>("LocationType",
          entities.InterstatePaymentAddress.LocationType);
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "othStateClsDte", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    System.Diagnostics.Debug.Assert(entities.AbsentParent.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableInt32(command, "croId", entities.AbsentParent.Identifier);
        db.SetNullableString(command, "croType", entities.AbsentParent.Type1);
        db.SetNullableString(
          command, "cspNumber", entities.AbsentParent.CspNumber);
        db.SetNullableString(
          command, "casNumber", entities.AbsentParent.CasNumber);
        db.SetInt32(
          command, "othrStateFipsCd", export.InterstateRequest.OtherStateFips);
        db.SetNullableString(
          command, "tribalAgency", export.InterstateRequest.TribalAgency ?? ""
          );
        db.SetNullableString(
          command, "country", export.InterstateRequest.Country ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest3()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest3",
      null,
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.CreatedBy = db.GetString(reader, 3);
        entities.InterstateRequest.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.InterstateRequest.LastUpdatedBy = db.GetString(reader, 5);
        entities.InterstateRequest.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 7);
        entities.InterstateRequest.CaseType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 9);
        entities.InterstateRequest.OtherStateCaseClosureReason =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.OtherStateCaseClosureDate =
          db.GetNullableDate(reader, 11);
        entities.InterstateRequest.CasINumber =
          db.GetNullableString(reader, 12);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 13);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 14);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 15);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 16);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 17);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 18);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequestHistory1()
  {
    entities.InterstateRequestHistory.Populated = false;

    return Read("ReadInterstateRequestHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory2()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.CreatedBy =
          db.GetNullableString(reader, 2);
        entities.InterstateRequestHistory.TransactionDirectionInd =
          db.GetString(reader, 3);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 4);
        entities.InterstateRequestHistory.ActionCode = db.GetString(reader, 5);
        entities.InterstateRequestHistory.FunctionalTypeCode =
          db.GetString(reader, 6);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 7);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 8);
        entities.InterstateRequestHistory.ActionResolutionDate =
          db.GetNullableDate(reader, 9);
        entities.InterstateRequestHistory.AttachmentIndicator =
          db.GetNullableString(reader, 10);
        entities.InterstateRequestHistory.Note =
          db.GetNullableString(reader, 11);
        entities.InterstateRequestHistory.Populated = true;

        return true;
      });
  }

  private void UpdateCase()
  {
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;

    entities.Case1.Populated = false;
    Update("UpdateCase",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetString(command, "numb", entities.Case1.Number);
      });

    entities.Case1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Case1.LastUpdatedBy = lastUpdatedBy;
    entities.Case1.Populated = true;
  }

  private void UpdateInterstateRequest()
  {
    var otherStateCaseId = import.InterstateRequest.OtherStateCaseId ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var otherStateCaseStatus = "O";
    var caseType = import.InterstateRequest.CaseType ?? "";
    var ksCaseInd = "N";
    var otherStateCaseClosureDate = local.Current.Date;

    entities.InterstateRequest.Populated = false;
    Update("UpdateInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "otherStateCasId", otherStateCaseId);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "othStCaseStatus", otherStateCaseStatus);
        db.SetNullableString(command, "caseType", caseType);
        db.SetString(command, "ksCaseInd", ksCaseInd);
        db.SetNullableString(command, "othStateClsRes", "");
        db.
          SetNullableDate(command, "othStateClsDte", otherStateCaseClosureDate);
          
        db.SetInt32(
          command, "identifier", entities.InterstateRequest.IntHGeneratedId);
      });

    entities.InterstateRequest.OtherStateCaseId = otherStateCaseId;
    entities.InterstateRequest.LastUpdatedBy = lastUpdatedBy;
    entities.InterstateRequest.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.InterstateRequest.OtherStateCaseStatus = otherStateCaseStatus;
    entities.InterstateRequest.CaseType = caseType;
    entities.InterstateRequest.KsCaseInd = ksCaseInd;
    entities.InterstateRequest.OtherStateCaseClosureReason = "";
    entities.InterstateRequest.OtherStateCaseClosureDate =
      otherStateCaseClosureDate;
    entities.InterstateRequest.Populated = true;
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
    /// A value of OtherState.
    /// </summary>
    [JsonPropertyName("otherState")]
    public Fips OtherState
    {
      get => otherState ??= new();
      set => otherState = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
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
    /// A value of CaseMarkedDuplicate.
    /// </summary>
    [JsonPropertyName("caseMarkedDuplicate")]
    public Common CaseMarkedDuplicate
    {
      get => caseMarkedDuplicate ??= new();
      set => caseMarkedDuplicate = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private Fips otherState;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequestHistory interstateRequestHistory;
    private CsePerson ap;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private Common caseMarkedDuplicate;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of IreqCreated.
    /// </summary>
    [JsonPropertyName("ireqCreated")]
    public DateWorkArea IreqCreated
    {
      get => ireqCreated ??= new();
      set => ireqCreated = value;
    }

    /// <summary>
    /// A value of IreqUpdated.
    /// </summary>
    [JsonPropertyName("ireqUpdated")]
    public DateWorkArea IreqUpdated
    {
      get => ireqUpdated ??= new();
      set => ireqUpdated = value;
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

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private DateWorkArea ireqCreated;
    private DateWorkArea ireqUpdated;
    private Case1 case1;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
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
    /// A value of InterstateProgram.
    /// </summary>
    [JsonPropertyName("interstateProgram")]
    public Program InterstateProgram
    {
      get => interstateProgram ??= new();
      set => interstateProgram = value;
    }

    /// <summary>
    /// A value of InterstateChild.
    /// </summary>
    [JsonPropertyName("interstateChild")]
    public CsePersonsWorkSet InterstateChild
    {
      get => interstateChild ??= new();
      set => interstateChild = value;
    }

    /// <summary>
    /// A value of InterstatePersonProgram.
    /// </summary>
    [JsonPropertyName("interstatePersonProgram")]
    public PersonProgram InterstatePersonProgram
    {
      get => interstatePersonProgram ??= new();
      set => interstatePersonProgram = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of InterstateExist.
    /// </summary>
    [JsonPropertyName("interstateExist")]
    public Common InterstateExist
    {
      get => interstateExist ??= new();
      set => interstateExist = value;
    }

    private Common found;
    private Infrastructure infrastructure;
    private Program interstateProgram;
    private CsePersonsWorkSet interstateChild;
    private PersonProgram interstatePersonProgram;
    private DateWorkArea max;
    private DateWorkArea zero;
    private DateWorkArea current;
    private Common interstateExist;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    /// <summary>
    /// A value of ExistingPersonProgram.
    /// </summary>
    [JsonPropertyName("existingPersonProgram")]
    public PersonProgram ExistingPersonProgram
    {
      get => existingPersonProgram ??= new();
      set => existingPersonProgram = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CaseRole Child1
    {
      get => child1 ??= new();
      set => child1 = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CsePerson Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    /// <summary>
    /// A value of InterstatePaymentAddress.
    /// </summary>
    [JsonPropertyName("interstatePaymentAddress")]
    public InterstatePaymentAddress InterstatePaymentAddress
    {
      get => interstatePaymentAddress ??= new();
      set => interstatePaymentAddress = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    private InterstateCase interstateCase;
    private Program existingProgram;
    private PersonProgram existingPersonProgram;
    private CaseUnit caseUnit;
    private Infrastructure infrastructure;
    private CaseRole child1;
    private CsePerson child2;
    private InterstatePaymentAddress interstatePaymentAddress;
    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequestHistory interstateRequestHistory;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseRole absentParent;
    private CsePerson ap;
  }
#endregion
}
