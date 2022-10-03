// Program: SI_PROCESS_INCOMING_CSI_REQUEST, ID: 371084283, model: 746.
// Short name: SWE02620
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_PROCESS_INCOMING_CSI_REQUEST.
/// </summary>
[Serializable]
public partial class SiProcessIncomingCsiRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PROCESS_INCOMING_CSI_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiProcessIncomingCsiRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiProcessIncomingCsiRequest.
  /// </summary>
  public SiProcessIncomingCsiRequest(IContext context, Import import,
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
    // -----------------------------------------------------------------
    //                    M A I N T E N A N C E   L O G
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------
    // 04/12/2001	M Ramirez	WR# 287		Initial Development
    // 09/14/2004             A Hockman            changes to remove '00000' 
    // inserted before case number since
    // 
    // specs call for number to be left justified with trailing spaces
    // 
    // -----------------------------------------------------------------
    local.Batch.Flag = "Y";

    if (IsEmpty(import.InterstateCase.KsCaseId))
    {
      local.InterstateCase.ActionReasonCode = "FUINF";
      local.InterstateMiscellaneous.StatusChangeCode = "O";
      local.InterstateMiscellaneous.InformationTextLine1 =
        "Missing Kansas Case Id";
      local.FuinfReason.Flag = "M";
    }
    else
    {
      local.Case1.Number = import.InterstateCase.KsCaseId ?? Spaces(10);

      if (ReadCase())
      {
        if (AsChar(entities.Case1.Status) == 'C' && !
          Lt(import.Current.Date, entities.Case1.StatusDate))
        {
          foreach(var item in ReadCsePerson6())
          {
            if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
            {
              local.InterstateCase.ActionReasonCode = "FUINF";
              local.InterstateMiscellaneous.StatusChangeCode = "O";
              local.InterstateMiscellaneous.InformationTextLine1 =
                "Protected for Family Violence";
              local.FuinfReason.Flag = "F";

              goto Test;
            }
          }
        }
        else
        {
          foreach(var item in ReadCsePerson5())
          {
            if (!IsEmpty(entities.CsePerson.FamilyViolenceIndicator))
            {
              local.InterstateCase.ActionReasonCode = "FUINF";
              local.InterstateMiscellaneous.StatusChangeCode = "O";
              local.InterstateMiscellaneous.InformationTextLine1 =
                "Protected for Family Violence";
              local.FuinfReason.Flag = "F";

              goto Test;
            }
          }
        }
      }
      else
      {
        local.InterstateCase.ActionReasonCode = "FUINF";
        local.InterstateMiscellaneous.StatusChangeCode = "O";
        local.InterstateMiscellaneous.InformationTextLine1 =
          "Not a IV-D case contact KSpaycenter at www.kspaycenter.com";
        local.FuinfReason.Flag = "I";
      }
    }

Test:

    if (Equal(local.InterstateCase.ActionReasonCode, "FUINF"))
    {
      // ----------------------------------------------------
      // Prepare CSI P FUINF
      // ----------------------------------------------------
      local.InterstateCase.Assign(import.InterstateCase);
      local.InterstateCase.ActionCode = "P";
      local.InterstateCase.ActionReasonCode = "FUINF";

      // ---------------------------------------------------------
      // Setup Transaction Header and Case Datablock
      // ---------------------------------------------------------
      UseSiGenCsenetTransactSerialNo();

      if (Equal(local.Case1.Number, import.InterstateCase.KsCaseId) && !
        IsEmpty(local.Case1.Number))
      {
        local.InterstateCase.KsCaseId = local.Case1.Number;
      }
      else
      {
        local.InterstateCase.KsCaseId = import.InterstateCase.KsCaseId ?? "";
      }

      local.InterstateCase.ActionResolutionDate = import.Current.Date;

      // -----------------------------------------------------------
      // Indicators set according to the incoming transaction are
      // Case ind, AP ID ind, AP Locate ind, Participant ind, Order ind
      // -----------------------------------------------------------
      local.InterstateCase.AttachmentsInd = "N";
      local.InterstateCase.CollectionDataInd = 0;
      local.InterstateCase.InformationInd = 1;
    }
    else
    {
      // ----------------------------------------------------
      // Prepare CSI P FSINF
      // ----------------------------------------------------
      local.InterstateCase.FunctionalTypeCode = "CSI";
      local.InterstateCase.ActionCode = "P";
      local.InterstateCase.ActionReasonCode = "FSINF";

      // ---------------------------------------------------------
      // Setup Transaction Header and Case Datablock
      // ---------------------------------------------------------
      // mjr
      // ---------------------------------------
      // 02/28/2002
      // PR125887 - Replaced code with call to CAB
      // ----------------------------------------------------
      local.InterstateCase.OtherFipsState =
        import.InterstateCase.OtherFipsState;
      local.InterstateCase.OtherFipsCounty =
        import.InterstateCase.OtherFipsCounty.GetValueOrDefault();
      local.InterstateCase.OtherFipsLocation =
        import.InterstateCase.OtherFipsLocation.GetValueOrDefault();
      local.InterstateCase.InterstateCaseId =
        import.InterstateCase.InterstateCaseId ?? "";
      UseSiGetDataInterstateCaseDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      // changed this to stop inserting extra zeroes in front of the case number
      // because ICR requires case numbers to be left justified with trailing
      // spaces.   A Hockman 9/14/04
      local.InterstateCase.KsCaseId = local.Case1.Number;

      // ---------------------------------------------------------
      // Determine which AP is the primary on the transaction
      // ---------------------------------------------------------
      if (AsChar(entities.Case1.Status) == 'C' && !
        Lt(import.Current.Date, entities.Case1.StatusDate))
      {
        local.TempDate.Date = entities.Case1.StatusDate;
      }
      else
      {
        local.TempDate.Date = import.Current.Date;
      }

      ReadCsePerson1();
      local.InterstateCase.ApIdentificationInd = 0;

      if (local.Count.Count < 1)
      {
      }
      else if (local.Count.Count > 1)
      {
        local.CsePersonsWorkSet.Number = "";

        if (ReadInterstateApIdentification())
        {
          local.CsePersonsWorkSet.Number = "";

          if (!IsEmpty(entities.InterstateApIdentification.Ssn) && !
            Equal(entities.InterstateApIdentification.Ssn, "000000000"))
          {
            local.CsePersonsWorkSet.Ssn =
              entities.InterstateApIdentification.Ssn ?? Spaces(9);
            local.AbendData.Assign(local.NullAbendData);
            UseEabReadCsePersonUsingSsn();

            if (!IsEmpty(local.AbendData.Type1))
            {
              if (Equal(local.AbendData.AdabasResponseCd, "0148"))
              {
                // ----------------------------------------------------
                // ADABAS unavailable.  No need to continue job
                // ----------------------------------------------------
                export.WriteError.Flag = "Y";
                export.AbendRollbackRequired.Flag = "A";
                export.Error.RptDetail = "ADABAS Unavailable";
              }
              else
              {
                // ----------------------------------------------------
                // Error occurred reading this individual
                // ----------------------------------------------------
                export.WriteError.Flag = "Y";
                export.Error.RptDetail = "ADABAS Error;  Person SSN = " + entities
                  .InterstateApIdentification.Ssn + ";  ADABAS return code = " +
                  local.AbendData.AdabasResponseCd;
              }

              return;
            }

            if (!IsEmpty(local.CsePersonsWorkSet.Number))
            {
              if (ReadCsePerson3())
              {
                local.PrimaryApCsePerson.Assign(entities.CsePerson);

                // ---------------------------------------------------------
                // Create AP ID Datablock for AP
                // ---------------------------------------------------------
                local.InterstateCase.ApIdentificationInd = 1;
              }
              else
              {
                // ---------------------------------------------------------
                // SSN specified was not for a person that is an AP on this Case
                // ---------------------------------------------------------
                local.CsePersonsWorkSet.Number = "";
              }
            }
          }

          if (IsEmpty(local.CsePersonsWorkSet.Number))
          {
            if (!IsEmpty(entities.InterstateApIdentification.AliasSsn1) && !
              Equal(entities.InterstateApIdentification.AliasSsn1, "000000000"))
            {
              local.CsePersonsWorkSet.Ssn =
                entities.InterstateApIdentification.AliasSsn1 ?? Spaces(9);
              local.AbendData.Assign(local.NullAbendData);
              UseEabReadCsePersonUsingSsn();

              if (!IsEmpty(local.AbendData.Type1))
              {
                if (Equal(local.AbendData.AdabasResponseCd, "0148"))
                {
                  // ----------------------------------------------------
                  // ADABAS unavailable.  No need to continue job
                  // ----------------------------------------------------
                  export.WriteError.Flag = "Y";
                  export.AbendRollbackRequired.Flag = "A";
                  export.Error.RptDetail = "ADABAS Unavailable";
                }
                else
                {
                  // ----------------------------------------------------
                  // Error occurred reading this individual
                  // ----------------------------------------------------
                  export.WriteError.Flag = "Y";
                  export.Error.RptDetail = "ADABAS Error;  Person SSN = " + entities
                    .InterstateApIdentification.AliasSsn1 + ";  ADABAS return code = " +
                    local.AbendData.AdabasResponseCd;
                }

                return;
              }

              if (!IsEmpty(local.CsePersonsWorkSet.Number))
              {
                if (ReadCsePerson3())
                {
                  local.PrimaryApCsePerson.Assign(entities.CsePerson);

                  // ---------------------------------------------------------
                  // Create AP ID Datablock for AP
                  // ---------------------------------------------------------
                  local.InterstateCase.ApIdentificationInd = 1;
                }
                else
                {
                  // ---------------------------------------------------------
                  // SSN specified was not for a person that is an AP on this 
                  // Case
                  // ---------------------------------------------------------
                  local.CsePersonsWorkSet.Number = "";
                }
              }
            }
          }

          if (IsEmpty(local.CsePersonsWorkSet.Number))
          {
            if (!IsEmpty(entities.InterstateApIdentification.AliasSsn2) && !
              Equal(entities.InterstateApIdentification.AliasSsn2, "000000000"))
            {
              local.CsePersonsWorkSet.Ssn =
                entities.InterstateApIdentification.AliasSsn2 ?? Spaces(9);
              local.AbendData.Assign(local.NullAbendData);
              UseEabReadCsePersonUsingSsn();

              if (!IsEmpty(local.AbendData.Type1))
              {
                if (Equal(local.AbendData.AdabasResponseCd, "0148"))
                {
                  // ----------------------------------------------------
                  // ADABAS unavailable.  No need to continue job
                  // ----------------------------------------------------
                  export.WriteError.Flag = "Y";
                  export.AbendRollbackRequired.Flag = "A";
                  export.Error.RptDetail = "ADABAS Unavailable";
                }
                else
                {
                  // ----------------------------------------------------
                  // Error occurred reading this individual
                  // ----------------------------------------------------
                  export.WriteError.Flag = "Y";
                  export.Error.RptDetail = "ADABAS Error;  Person SSN = " + entities
                    .InterstateApIdentification.AliasSsn2 + ";  ADABAS return code = " +
                    local.AbendData.AdabasResponseCd;
                }

                return;
              }

              if (!IsEmpty(local.CsePersonsWorkSet.Number))
              {
                if (ReadCsePerson3())
                {
                  local.PrimaryApCsePerson.Assign(entities.CsePerson);

                  // ---------------------------------------------------------
                  // Create AP ID Datablock for AP
                  // ---------------------------------------------------------
                  local.InterstateCase.ApIdentificationInd = 1;
                }
                else
                {
                  // ---------------------------------------------------------
                  // SSN specified was not for a person that is an AP on this 
                  // Case
                  // ---------------------------------------------------------
                  local.CsePersonsWorkSet.Number = "";
                }
              }
            }
          }
        }
        else
        {
          // ---------------------------------------------------------
          // Incoming transaction did not specify a person
          // ---------------------------------------------------------
        }

        if (IsEmpty(local.CsePersonsWorkSet.Number))
        {
          // ---------------------------------------------------------
          // Incoming transaction did not specify a person, or
          // the person specified was not an AP on the Case
          // ---------------------------------------------------------
          // ---------------------------------------------------------
          // Pick the first AP
          // ---------------------------------------------------------
          if (ReadCsePerson2())
          {
            local.PrimaryApCsePerson.Assign(entities.CsePerson);

            // ---------------------------------------------------------
            // Create AP ID Datablock for AP
            // ---------------------------------------------------------
            local.InterstateCase.ApIdentificationInd = 1;
          }
        }
      }
      else
      {
        // ---------------------------------------------------------
        // Find AP
        // ---------------------------------------------------------
        if (ReadCsePerson4())
        {
          local.PrimaryApCsePerson.Assign(entities.CsePerson);

          // ---------------------------------------------------------
          // Create AP ID Datablock for AP
          // ---------------------------------------------------------
          local.InterstateCase.ApIdentificationInd = 1;
        }
        else
        {
          export.WriteError.Flag = "Y";
          export.Error.RptDetail =
            "Active AP not found for Case; Case Number =" + entities
            .Case1.Number;

          return;
        }
      }

      // ---------------------------------------------------------
      // Setup AP ID Datablock
      // ---------------------------------------------------------
      if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
      {
        local.PrimaryApCsePersonsWorkSet.Number =
          local.PrimaryApCsePerson.Number;
        UseSiGetDataInterstateApIdDb();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.WriteError.Flag = "Y";

          if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
          {
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "ADABAS Unavailable";
          }
          else
          {
            export.Error.RptDetail = "ADABAS Error;  CSE Person = " + local
              .PrimaryApCsePersonsWorkSet.Number;
          }

          return;
        }

        // ---------------------------------------------------------
        // Setup AP Locate Datablock
        // ---------------------------------------------------------
        UseSiGetDataInterstateApLocDb();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.WriteError.Flag = "Y";

          if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
          {
            export.AbendRollbackRequired.Flag = "A";
            export.Error.RptDetail = "ADABAS Unavailable";
          }
          else
          {
            export.Error.RptDetail = "ADABAS Error;  CSE Person = " + local
              .PrimaryApCsePersonsWorkSet.Number;
          }

          return;
        }
      }

      // ---------------------------------------------------------
      // Setup Participant Datablocks
      // NOTE:  For a Case with multiple APs, the first AP goes
      // into the AP ID and AP Locate datablocks.  The remaining
      // APs go in the Participant datablocks, provided there is
      // space available.
      // ---------------------------------------------------------
      // mjr
      // ---------------------------------------
      // 02/28/2002
      // PR125887 - Replaced code with call to CAB
      // ----------------------------------------------------
      UseSiGetDataInterstatePartDbs();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.WriteError.Flag = "Y";

        if (IsExitState("ACO_ADABAS_UNAVAILABLE"))
        {
          export.AbendRollbackRequired.Flag = "A";
          export.Error.RptDetail = "ADABAS Unavailable";
        }
        else
        {
          export.Error.RptDetail = "ADABAS Error;  CSE Person = " + local
            .PrimaryApCsePersonsWorkSet.Number;
        }

        return;
      }

      // ---------------------------------------------------------
      // Setup Order Datablock
      // ---------------------------------------------------------
      if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
      {
        UseSiGetDataInterstateOrderDb();
      }
    }

    // ---------------------------------------------------------
    // Create Interstate Case Datablock
    // ---------------------------------------------------------
    UseSiCreateInterstateCase();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      export.WriteError.Flag = "Y";
      export.Error.RptDetail =
        "Error occurred trying to ADD case datablock; Case Number = " + entities
        .Case1.Number;

      return;
    }

    // ---------------------------------------------------------
    // Create Transaction Header
    // ---------------------------------------------------------
    UseSiCreateOgCsenetEnvelop();

    if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
    {
      export.WriteError.Flag = "Y";
      export.AbendRollbackRequired.Flag = "R";
      export.Error.RptDetail =
        "Error occurred trying to ADD transaction header; Case Number = " + entities
        .Case1.Number;

      return;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }

    // ---------------------------------------------------------
    // Create AP ID datablock
    // ---------------------------------------------------------
    if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
    {
      UseSiCreateInterstateApId();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.WriteError.Flag = "Y";
        export.AbendRollbackRequired.Flag = "R";
        export.Error.RptDetail =
          "Error occurred trying to ADD ap id datablock; CSE Person = " + local
          .PrimaryApCsePerson.Number;

        return;
      }
    }

    // ---------------------------------------------------------
    // Create AP Locate datablock
    // ---------------------------------------------------------
    if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
    {
      UseSiCreateInterstateApLocate();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.WriteError.Flag = "Y";
        export.AbendRollbackRequired.Flag = "R";
        export.Error.RptDetail =
          "Error occurred trying to ADD ap locate datablock; CSE Person = " + local
          .PrimaryApCsePerson.Number;

        return;
      }
    }

    // ---------------------------------------------------------
    // Create Participant Datablocks
    // ---------------------------------------------------------
    if (local.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0)
    {
      for(local.Participants.Index = 0; local.Participants.Index < local
        .Participants.Count; ++local.Participants.Index)
      {
        if (!local.Participants.CheckSize())
        {
          break;
        }

        UseSiCreateInterstateParticipant();

        if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "R";
          export.Error.RptDetail =
            "Error occurred trying to ADD participant datablock; CSE Person = " +
            local.Participants.Item.GlocalParticipant.Number;

          return;
        }
        else
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

      local.Participants.CheckIndex();
    }

    // ---------------------------------------------------------
    // Create Order datablocks
    // ---------------------------------------------------------
    if (local.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
    {
      for(local.Orders.Index = 0; local.Orders.Index < local.Orders.Count; ++
        local.Orders.Index)
      {
        if (!local.Orders.CheckSize())
        {
          break;
        }

        UseSiCreateInterstateOrder();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.WriteError.Flag = "Y";
          export.AbendRollbackRequired.Flag = "R";
          export.Error.RptDetail =
            "Error occurred trying to ADD order datablock; Case Number = " + entities
            .Case1.Number;

          return;
        }
      }

      local.Orders.CheckIndex();
    }

    // ---------------------------------------------------------
    // Create Miscellaneous datablock
    // ---------------------------------------------------------
    if (local.InterstateCase.InformationInd.GetValueOrDefault() > 0)
    {
      UseSiCreateInterstateMisc();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.WriteError.Flag = "Y";
        export.AbendRollbackRequired.Flag = "R";
        export.Error.RptDetail =
          "Error occurred trying to ADD miscellaneous datablock; Case Number = " +
          entities.Case1.Number;

        return;
      }
    }

    MoveInterstateCase2(local.InterstateCase, export.Created);

    // ---------------------------------------------------------
    // Increment counts if there are no Rollback situations
    // ---------------------------------------------------------
    if (Equal(local.InterstateCase.ActionReasonCode, "FSINF"))
    {
      ++import.ExpFsinf.Count;
    }
    else
    {
      switch(AsChar(local.FuinfReason.Flag))
      {
        case 'F':
          ++import.ExpFuinfFv.Count;

          break;
        case 'I':
          ++import.ExpFuinfInvalidCase.Count;

          break;
        case 'M':
          ++import.ExpFuinfMissingCase.Count;

          break;
        default:
          break;
      }
    }

    ++import.ExpTransaction.Count;
    import.ExpCaseDb.Count += local.InterstateCase.CaseDataInd.
      GetValueOrDefault();
    import.ExpApidDb.Count += local.InterstateCase.ApIdentificationInd.
      GetValueOrDefault();
    import.ExpApLocateDb.Count += local.InterstateCase.ApLocateDataInd.
      GetValueOrDefault();
    import.ExpParticipantDb.Count += local.InterstateCase.ParticipantDataInd.
      GetValueOrDefault();
    import.ExpOrderDb.Count += local.InterstateCase.OrderDataInd.
      GetValueOrDefault();
    import.ExpMiscDb.Count += local.InterstateCase.InformationInd.
      GetValueOrDefault();

    // ---------------------------------------------------------
    // Checkpoint Update is the max of all the counts
    // ---------------------------------------------------------
    import.ExpCheckpointUpdate.Count = import.ExpTransaction.Count;

    if (import.ExpParticipantDb.Count > import.ExpCheckpointUpdate.Count)
    {
      import.ExpCheckpointUpdate.Count = import.ExpParticipantDb.Count;
    }

    if (import.ExpOrderDb.Count > import.ExpCheckpointUpdate.Count)
    {
      import.ExpCheckpointUpdate.Count = import.ExpOrderDb.Count;
    }
  }

  private static void MoveAbendData(AbendData source, AbendData target)
  {
    target.AdabasFileNumber = source.AdabasFileNumber;
    target.AdabasFileAction = source.AdabasFileAction;
    target.AdabasResponseCd = source.AdabasResponseCd;
    target.CicsResourceNm = source.CicsResourceNm;
    target.CicsFunctionCd = source.CicsFunctionCd;
    target.CicsResponseCd = source.CicsResponseCd;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveGroupToOrders(SiGetDataInterstateOrderDb.Export.
    GroupGroup source, Local.OrdersGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveGroupToParticipants(SiGetDataInterstatePartDbs.Export.
    GroupGroup source, Local.ParticipantsGroup target)
  {
    target.GlocalParticipant.Number = source.GexportParticipant.Number;
    target.G.Assign(source.G);
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.AttachmentsInd = source.AttachmentsInd;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseEabReadCsePersonUsingSsn()
  {
    var useImport = new EabReadCsePersonUsingSsn.Import();
    var useExport = new EabReadCsePersonUsingSsn.Export();

    useImport.CsePersonsWorkSet.Ssn = local.CsePersonsWorkSet.Ssn;
    useExport.AbendData.Assign(local.AbendData);
    useExport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(EabReadCsePersonUsingSsn.Execute, useImport, useExport);

    MoveAbendData(useExport.AbendData, local.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private void UseSiCreateInterstateApId()
  {
    var useImport = new SiCreateInterstateApId.Import();
    var useExport = new SiCreateInterstateApId.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);

    Call(SiCreateInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApLocate()
  {
    var useImport = new SiCreateInterstateApLocate.Import();
    var useExport = new SiCreateInterstateApLocate.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);

    Call(SiCreateInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateCase()
  {
    var useImport = new SiCreateInterstateCase.Import();
    var useExport = new SiCreateInterstateCase.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiCreateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateMisc()
  {
    var useImport = new SiCreateInterstateMisc.Import();
    var useExport = new SiCreateInterstateMisc.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);

    Call(SiCreateInterstateMisc.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateOrder()
  {
    var useImport = new SiCreateInterstateOrder.Import();
    var useExport = new SiCreateInterstateOrder.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateSupportOrder.Assign(local.Orders.Item.G);

    Call(SiCreateInterstateOrder.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateParticipant()
  {
    var useImport = new SiCreateInterstateParticipant.Import();
    var useExport = new SiCreateInterstateParticipant.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateParticipant.Assign(local.Participants.Item.G);

    Call(SiCreateInterstateParticipant.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGenCsenetTransactSerialNo()
  {
    var useImport = new SiGenCsenetTransactSerialNo.Import();
    var useExport = new SiGenCsenetTransactSerialNo.Export();

    Call(SiGenCsenetTransactSerialNo.Execute, useImport, useExport);

    MoveInterstateCase2(useExport.InterstateCase, local.InterstateCase);
  }

  private void UseSiGetDataInterstateApIdDb()
  {
    var useImport = new SiGetDataInterstateApIdDb.Import();
    var useExport = new SiGetDataInterstateApIdDb.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Ap.Number = local.PrimaryApCsePersonsWorkSet.Number;

    Call(SiGetDataInterstateApIdDb.Execute, useImport, useExport);

    local.InterstateCase.ApIdentificationInd =
      useExport.InterstateCase.ApIdentificationInd;
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
  }

  private void UseSiGetDataInterstateApLocDb()
  {
    var useImport = new SiGetDataInterstateApLocDb.Import();
    var useExport = new SiGetDataInterstateApLocDb.Export();

    useImport.Current.Date = import.Current.Date;
    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Ap.Assign(local.PrimaryApCsePersonsWorkSet);

    Call(SiGetDataInterstateApLocDb.Execute, useImport, useExport);

    local.InterstateCase.ApLocateDataInd =
      useExport.InterstateCase.ApLocateDataInd;
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
  }

  private void UseSiGetDataInterstateCaseDb()
  {
    var useImport = new SiGetDataInterstateCaseDb.Import();
    var useExport = new SiGetDataInterstateCaseDb.Export();

    useImport.Current.Date = import.Current.Date;
    useImport.Case1.Number = local.Case1.Number;
    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);

    Call(SiGetDataInterstateCaseDb.Execute, useImport, useExport);

    local.InterstateCase.Assign(useExport.InterstateCase);
  }

  private void UseSiGetDataInterstateOrderDb()
  {
    var useImport = new SiGetDataInterstateOrderDb.Import();
    var useExport = new SiGetDataInterstateOrderDb.Export();

    useImport.Zdel.FunctionalTypeCode = local.InterstateCase.FunctionalTypeCode;
    useImport.ZdelImportPrimaryAp.Number = local.PrimaryApCsePerson.Number;
    useImport.Case1.Number = local.Case1.Number;
    useImport.Current.Date = import.Current.Date;

    Call(SiGetDataInterstateOrderDb.Execute, useImport, useExport);

    local.InterstateCase.OrderDataInd = useExport.InterstateCase.OrderDataInd;
    useExport.Group.CopyTo(local.Orders, MoveGroupToOrders);
  }

  private void UseSiGetDataInterstatePartDbs()
  {
    var useImport = new SiGetDataInterstatePartDbs.Import();
    var useExport = new SiGetDataInterstatePartDbs.Export();

    useImport.Case1.Number = local.Case1.Number;
    useImport.PrimaryAp.Number = local.PrimaryApCsePersonsWorkSet.Number;
    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Current.Date = import.Current.Date;

    Call(SiGetDataInterstatePartDbs.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Participants, MoveGroupToParticipants);
    local.InterstateCase.ParticipantDataInd =
      useExport.InterstateCase.ParticipantDataInd;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 2);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.TempDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        local.Count.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.TempDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 14);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 19);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 20);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", local.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "startDate", local.TempDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 14);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 19);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 20);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.TempDate.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", local.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 14);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 19);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 20);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson5()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 14);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 19);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 20);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson6()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate", entities.Case1.StatusDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 3);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 4);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 5);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 6);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 7);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 8);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 9);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 10);
        entities.CsePerson.Race = db.GetNullableString(reader, 11);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 12);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 13);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 14);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 15);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 16);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 18);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 19);
        entities.CsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 20);
        entities.CsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 21);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadInterstateApIdentification()
  {
    entities.InterstateApIdentification.Populated = false;

    return Read("ReadInterstateApIdentification",
      (db, command) =>
      {
        db.SetInt64(
          command, "ccaTransSerNum", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "ccaTransactionDt",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateApIdentification.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.InterstateApIdentification.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.InterstateApIdentification.AliasSsn2 =
          db.GetNullableString(reader, 2);
        entities.InterstateApIdentification.AliasSsn1 =
          db.GetNullableString(reader, 3);
        entities.InterstateApIdentification.OtherIdInfo =
          db.GetNullableString(reader, 4);
        entities.InterstateApIdentification.EyeColor =
          db.GetNullableString(reader, 5);
        entities.InterstateApIdentification.HairColor =
          db.GetNullableString(reader, 6);
        entities.InterstateApIdentification.Weight =
          db.GetNullableInt32(reader, 7);
        entities.InterstateApIdentification.HeightIn =
          db.GetNullableInt32(reader, 8);
        entities.InterstateApIdentification.HeightFt =
          db.GetNullableInt32(reader, 9);
        entities.InterstateApIdentification.PlaceOfBirth =
          db.GetNullableString(reader, 10);
        entities.InterstateApIdentification.Ssn =
          db.GetNullableString(reader, 11);
        entities.InterstateApIdentification.Race =
          db.GetNullableString(reader, 12);
        entities.InterstateApIdentification.Sex =
          db.GetNullableString(reader, 13);
        entities.InterstateApIdentification.DateOfBirth =
          db.GetNullableDate(reader, 14);
        entities.InterstateApIdentification.NameSuffix =
          db.GetNullableString(reader, 15);
        entities.InterstateApIdentification.NameFirst =
          db.GetString(reader, 16);
        entities.InterstateApIdentification.NameLast =
          db.GetNullableString(reader, 17);
        entities.InterstateApIdentification.MiddleName =
          db.GetNullableString(reader, 18);
        entities.InterstateApIdentification.PossiblyDangerous =
          db.GetNullableString(reader, 19);
        entities.InterstateApIdentification.MaidenName =
          db.GetNullableString(reader, 20);
        entities.InterstateApIdentification.MothersMaidenOrFathersName =
          db.GetNullableString(reader, 21);
        entities.InterstateApIdentification.Populated = true;
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
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of ExpFsinf.
    /// </summary>
    [JsonPropertyName("expFsinf")]
    public Common ExpFsinf
    {
      get => expFsinf ??= new();
      set => expFsinf = value;
    }

    /// <summary>
    /// A value of ExpFuinfMissingCase.
    /// </summary>
    [JsonPropertyName("expFuinfMissingCase")]
    public Common ExpFuinfMissingCase
    {
      get => expFuinfMissingCase ??= new();
      set => expFuinfMissingCase = value;
    }

    /// <summary>
    /// A value of ExpFuinfInvalidCase.
    /// </summary>
    [JsonPropertyName("expFuinfInvalidCase")]
    public Common ExpFuinfInvalidCase
    {
      get => expFuinfInvalidCase ??= new();
      set => expFuinfInvalidCase = value;
    }

    /// <summary>
    /// A value of ExpFuinfFv.
    /// </summary>
    [JsonPropertyName("expFuinfFv")]
    public Common ExpFuinfFv
    {
      get => expFuinfFv ??= new();
      set => expFuinfFv = value;
    }

    /// <summary>
    /// A value of ExpTransaction.
    /// </summary>
    [JsonPropertyName("expTransaction")]
    public Common ExpTransaction
    {
      get => expTransaction ??= new();
      set => expTransaction = value;
    }

    /// <summary>
    /// A value of ExpCaseDb.
    /// </summary>
    [JsonPropertyName("expCaseDb")]
    public Common ExpCaseDb
    {
      get => expCaseDb ??= new();
      set => expCaseDb = value;
    }

    /// <summary>
    /// A value of ExpApidDb.
    /// </summary>
    [JsonPropertyName("expApidDb")]
    public Common ExpApidDb
    {
      get => expApidDb ??= new();
      set => expApidDb = value;
    }

    /// <summary>
    /// A value of ExpApLocateDb.
    /// </summary>
    [JsonPropertyName("expApLocateDb")]
    public Common ExpApLocateDb
    {
      get => expApLocateDb ??= new();
      set => expApLocateDb = value;
    }

    /// <summary>
    /// A value of ExpParticipantDb.
    /// </summary>
    [JsonPropertyName("expParticipantDb")]
    public Common ExpParticipantDb
    {
      get => expParticipantDb ??= new();
      set => expParticipantDb = value;
    }

    /// <summary>
    /// A value of ExpOrderDb.
    /// </summary>
    [JsonPropertyName("expOrderDb")]
    public Common ExpOrderDb
    {
      get => expOrderDb ??= new();
      set => expOrderDb = value;
    }

    /// <summary>
    /// A value of ExpMiscDb.
    /// </summary>
    [JsonPropertyName("expMiscDb")]
    public Common ExpMiscDb
    {
      get => expMiscDb ??= new();
      set => expMiscDb = value;
    }

    /// <summary>
    /// A value of ExpCheckpointUpdate.
    /// </summary>
    [JsonPropertyName("expCheckpointUpdate")]
    public Common ExpCheckpointUpdate
    {
      get => expCheckpointUpdate ??= new();
      set => expCheckpointUpdate = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    private InterstateCase interstateCase;
    private Common expFsinf;
    private Common expFuinfMissingCase;
    private Common expFuinfInvalidCase;
    private Common expFuinfFv;
    private Common expTransaction;
    private Common expCaseDb;
    private Common expApidDb;
    private Common expApLocateDb;
    private Common expParticipantDb;
    private Common expOrderDb;
    private Common expMiscDb;
    private Common expCheckpointUpdate;
    private DateWorkArea current;
    private DateWorkArea max;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Created.
    /// </summary>
    [JsonPropertyName("created")]
    public InterstateCase Created
    {
      get => created ??= new();
      set => created = value;
    }

    /// <summary>
    /// A value of AbendRollbackRequired.
    /// </summary>
    [JsonPropertyName("abendRollbackRequired")]
    public Common AbendRollbackRequired
    {
      get => abendRollbackRequired ??= new();
      set => abendRollbackRequired = value;
    }

    /// <summary>
    /// A value of WriteError.
    /// </summary>
    [JsonPropertyName("writeError")]
    public Common WriteError
    {
      get => writeError ??= new();
      set => writeError = value;
    }

    /// <summary>
    /// A value of Error.
    /// </summary>
    [JsonPropertyName("error")]
    public EabReportSend Error
    {
      get => error ??= new();
      set => error = value;
    }

    private InterstateCase created;
    private Common abendRollbackRequired;
    private Common writeError;
    private EabReportSend error;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A SsnGroup group.</summary>
    [Serializable]
    public class SsnGroup
    {
      /// <summary>
      /// A value of GlocalSsn.
      /// </summary>
      [JsonPropertyName("glocalSsn")]
      public CsePersonsWorkSet GlocalSsn
      {
        get => glocalSsn ??= new();
        set => glocalSsn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet glocalSsn;
    }

    /// <summary>A AliasGroup group.</summary>
    [Serializable]
    public class AliasGroup
    {
      /// <summary>
      /// A value of GlocalAlias.
      /// </summary>
      [JsonPropertyName("glocalAlias")]
      public CsePersonsWorkSet GlocalAlias
      {
        get => glocalAlias ??= new();
        set => glocalAlias = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet glocalAlias;
    }

    /// <summary>A OrdersGroup group.</summary>
    [Serializable]
    public class OrdersGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateSupportOrder G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder g;
    }

    /// <summary>A ParticipantsGroup group.</summary>
    [Serializable]
    public class ParticipantsGroup
    {
      /// <summary>
      /// A value of GlocalParticipant.
      /// </summary>
      [JsonPropertyName("glocalParticipant")]
      public CsePerson GlocalParticipant
      {
        get => glocalParticipant ??= new();
        set => glocalParticipant = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateParticipant G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePerson glocalParticipant;
      private InterstateParticipant g;
    }

    /// <summary>
    /// A value of TempDate.
    /// </summary>
    [JsonPropertyName("tempDate")]
    public DateWorkArea TempDate
    {
      get => tempDate ??= new();
      set => tempDate = value;
    }

    /// <summary>
    /// A value of FuinfReason.
    /// </summary>
    [JsonPropertyName("fuinfReason")]
    public Common FuinfReason
    {
      get => fuinfReason ??= new();
      set => fuinfReason = value;
    }

    /// <summary>
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public InterstateSupportOrder Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// Gets a value of Ssn.
    /// </summary>
    [JsonIgnore]
    public Array<SsnGroup> Ssn => ssn ??= new(SsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ssn for json serialization.
    /// </summary>
    [JsonPropertyName("ssn")]
    [Computed]
    public IList<SsnGroup> Ssn_Json
    {
      get => ssn;
      set => Ssn.Assign(value);
    }

    /// <summary>
    /// Gets a value of Alias.
    /// </summary>
    [JsonIgnore]
    public Array<AliasGroup> Alias => alias ??= new(AliasGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Alias for json serialization.
    /// </summary>
    [JsonPropertyName("alias")]
    [Computed]
    public IList<AliasGroup> Alias_Json
    {
      get => alias;
      set => Alias.Assign(value);
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of PrimaryApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("primaryApCsePersonsWorkSet")]
    public CsePersonsWorkSet PrimaryApCsePersonsWorkSet
    {
      get => primaryApCsePersonsWorkSet ??= new();
      set => primaryApCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// Gets a value of Orders.
    /// </summary>
    [JsonIgnore]
    public Array<OrdersGroup> Orders => orders ??= new(OrdersGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Orders for json serialization.
    /// </summary>
    [JsonPropertyName("orders")]
    [Computed]
    public IList<OrdersGroup> Orders_Json
    {
      get => orders;
      set => Orders.Assign(value);
    }

    /// <summary>
    /// A value of NullAbendData.
    /// </summary>
    [JsonPropertyName("nullAbendData")]
    public AbendData NullAbendData
    {
      get => nullAbendData ??= new();
      set => nullAbendData = value;
    }

    /// <summary>
    /// Gets a value of Participants.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantsGroup> Participants => participants ??= new(
      ParticipantsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Participants for json serialization.
    /// </summary>
    [JsonPropertyName("participants")]
    [Computed]
    public IList<ParticipantsGroup> Participants_Json
    {
      get => participants;
      set => Participants.Assign(value);
    }

    /// <summary>
    /// A value of PrimaryApCsePerson.
    /// </summary>
    [JsonPropertyName("primaryApCsePerson")]
    public CsePerson PrimaryApCsePerson
    {
      get => primaryApCsePerson ??= new();
      set => primaryApCsePerson = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
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
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
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

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

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
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NullCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("nullCsePersonsWorkSet")]
    public CsePersonsWorkSet NullCsePersonsWorkSet
    {
      get => nullCsePersonsWorkSet ??= new();
      set => nullCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    private DateWorkArea tempDate;
    private Common fuinfReason;
    private InterstateSupportOrder temp;
    private Array<SsnGroup> ssn;
    private Array<AliasGroup> alias;
    private Common batch;
    private CsePersonsWorkSet primaryApCsePersonsWorkSet;
    private DateWorkArea nullDateWorkArea;
    private Array<OrdersGroup> orders;
    private AbendData nullAbendData;
    private Array<ParticipantsGroup> participants;
    private CsePerson primaryApCsePerson;
    private Common count;
    private Case1 case1;
    private Program program;
    private InterstateCase interstateCase;
    private AbendData abendData;
    private Common found;
    private InterstateApLocate interstateApLocate;
    private InterstateApIdentification interstateApIdentification;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet nullCsePersonsWorkSet;
    private InterstateMiscellaneous interstateMiscellaneous;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of Jclass.
    /// </summary>
    [JsonPropertyName("jclass")]
    public LegalAction Jclass
    {
      get => jclass ??= new();
      set => jclass = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of ServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("serviceProviderAddress")]
    public ServiceProviderAddress ServiceProviderAddress
    {
      get => serviceProviderAddress ??= new();
      set => serviceProviderAddress = value;
    }

    /// <summary>
    /// A value of CaseAssignment.
    /// </summary>
    [JsonPropertyName("caseAssignment")]
    public CaseAssignment CaseAssignment
    {
      get => caseAssignment ??= new();
      set => caseAssignment = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private ObligationType obligationType;
    private LegalActionPerson legalActionPerson;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionCaseRole legalActionCaseRole;
    private LegalAction jclass;
    private LegalActionDetail legalActionDetail;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private OfficeAddress officeAddress;
    private ServiceProviderAddress serviceProviderAddress;
    private CaseAssignment caseAssignment;
    private CaseRole caseRole;
    private Case1 case1;
    private Office office;
    private OfficeServiceProvider officeServiceProvider;
    private ServiceProvider serviceProvider;
    private InterstateApIdentification interstateApIdentification;
    private CsePersonLicense csePersonLicense;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private CsePerson csePerson;
    private EmployerAddress employerAddress;
    private Employer employer;
    private IncomeSource incomeSource;
    private CsePersonAddress csePersonAddress;
    private InterstateCase interstateCase;
  }
#endregion
}
