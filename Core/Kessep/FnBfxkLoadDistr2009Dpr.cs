// Program: FN_BFXK_LOAD_DISTR_2009_DPR, ID: 371407863, model: 746.
// Short name: SWEFFXKB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXK_LOAD_DISTR_2009_DPR.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxkLoadDistr2009Dpr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXK_LOAD_DISTR_2009_DPR program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxkLoadDistr2009Dpr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxkLoadDistr2009Dpr.
  /// </summary>
  public FnBfxkLoadDistr2009Dpr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // This is a one time program to load new distribution policy rules for 
    // CQ4387 (Distribution 2009)
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // ---------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ------------	---------	
    // -----------------------------------------------------------------
    // 12/24/08  GVandy	CQ4387		Initial development for Distribution 2009.
    // 03/11/21  GVandy	CQ69409		Allow distribution policy rule effective date 
    // as a file value.
    // 					Will be using this program to add revised distribution
    // 					policy rules for U type collections effective
    // 					04-01-2021.
    // ---------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Dist2009StartDate.Date = new DateTime(2009, 10, 1);
    local.MaxDate.Date = new DateTime(2099, 12, 31);

    if (ReadDistributionPolicy1())
    {
      local.MaxDistributionPolicy.SystemGeneratedIdentifier =
        entities.DistributionPolicy1.SystemGeneratedIdentifier;
    }

    // -- Open the control report.
    local.EabFileHandling.Action = "OPEN";
    UseCabControlReport2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -- Open file containing the distribution policy rules.
    local.FileNumber.Count = 0;
    local.FileRecord.FileInstruction = "OPEN";
    UseFnB721ExtReadFile();

    if (!Equal(local.FileRecord.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_OPENING_EXT_FILE";

      return;
    }

    // -- Read first record from file...
    local.FileRecord.Assign(local.Null1);
    local.FileRecord.FileInstruction = "READ";
    UseFnB721ExtReadFile();

    if (!Equal(local.FileRecord.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_READING_EXT_FILE";

      return;
    }

    do
    {
      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
      local.EabReportSend.RptDetail = local.FileRecord.TextLine80;
      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }

      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
      switch(TrimEnd(Substring(local.FileRecord.TextLine80, 1, 3)))
      {
        case "EDT":
          // --------------------------------------------------------------
          // -- Override the default distribution policy rule effective date.
          // --------------------------------------------------------------
          local.Dist2009StartDate.Date =
            StringToDate(Substring(
              local.FileRecord.TextLine80, External.TextLine80_MaxLength, 4,
            10));

          // -- Read next record...
          local.FileRecord.Assign(local.Null1);
          local.FileRecord.FileInstruction = "READ";
          UseFnB721ExtReadFile();

          if (!Equal(local.FileRecord.TextReturnCode, "OK") && !
            Equal(local.FileRecord.TextReturnCode, "EF"))
          {
            ExitState = "OE0000_ERROR_READING_EXT_FILE";

            return;
          }

          break;
        case "DPO":
          // --------------------------------------------------------------
          // -- Distribution policy record.
          // --------------------------------------------------------------
          // -- Initialize groups.
          UseFnBfxkInitGroups();
          local.DistributionPolicy.Index = -1;
          local.DistributionPolicy.Count = 0;

          // -- Load all the collection types for the distribution policy into a
          // group...
          local.I.Count = 4;

          for(var limit = Length(TrimEnd(local.FileRecord.TextLine80)); local
            .I.Count <= limit; ++local.I.Count)
          {
            if (local.I.Count == 4)
            {
              ++local.DistributionPolicy.Index;
              local.DistributionPolicy.CheckSize();

              local.DistributionPolicy.Update.GrCollectionType.Code =
                Substring(local.FileRecord.TextLine80, local.I.Count, 1);
            }
            else if (CharAt(local.FileRecord.TextLine80, local.I.Count) == ',')
            {
              ++local.DistributionPolicy.Index;
              local.DistributionPolicy.CheckSize();

              continue;
            }
            else
            {
              local.DistributionPolicy.Update.GrCollectionType.Code =
                Substring(local.FileRecord.TextLine80, local.I.Count, 1);
            }

            if (ReadDistributionPolicy2())
            {
              if (Lt(local.Dist2009StartDate.Date, entities.Old.DiscontinueDt))
              {
                // -- Set the end date of the old distribution policy record to 
                // 09/30/2009.
                try
                {
                  UpdateDistributionPolicy();
                  ++local.NumDistPolicyUpdates.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      local.EabReportSend.RptDetail = "Error #1";
                      local.EabFileHandling.Action = "WRITE";
                      UseCabControlReport1();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                        return;
                      }

                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      local.EabReportSend.RptDetail = "Error #2";
                      local.EabFileHandling.Action = "WRITE";
                      UseCabControlReport1();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                        return;
                      }

                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }
            }

            if (!entities.Old.Populated)
            {
              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
              local.EabReportSend.RptDetail = "Error #3";
              local.EabFileHandling.Action = "WRITE";
              UseCabControlReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                return;
              }

              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            // -- Setup new distribution policy record with effective date on 10
            // /01/2009.
            local.DistributionPolicy.Update.GrDistributionPolicy.CreatedBy =
              global.UserId;
            local.DistributionPolicy.Update.GrDistributionPolicy.CreatedTmst =
              Now();
            local.DistributionPolicy.Update.GrDistributionPolicy.Description =
              entities.Old.Description;
            local.DistributionPolicy.Update.GrDistributionPolicy.DiscontinueDt =
              local.MaxDate.Date;
            local.DistributionPolicy.Update.GrDistributionPolicy.EffectiveDt =
              local.Dist2009StartDate.Date;
            local.DistributionPolicy.Update.GrDistributionPolicy.Name =
              entities.Old.Name;

            // --  Set the system generated id to max distribution policy system
            // generated id + 1
            ++local.MaxDistributionPolicy.SystemGeneratedIdentifier;
            local.DistributionPolicy.Update.GrDistributionPolicy.
              SystemGeneratedIdentifier =
                local.MaxDistributionPolicy.SystemGeneratedIdentifier;
          }

          local.DistPolicyRule.Index = -1;
          local.DistPolicyRule.Count = 0;
          local.MaxDistributionPolicyRule.SystemGeneratedIdentifier = 0;

          do
          {
            // -- Read next record...
            local.FileRecord.Assign(local.Null1);
            local.FileRecord.FileInstruction = "READ";
            UseFnB721ExtReadFile();

            if (!Equal(local.FileRecord.TextReturnCode, "OK") && !
              Equal(local.FileRecord.TextReturnCode, "EF"))
            {
              ExitState = "OE0000_ERROR_READING_EXT_FILE";

              return;
            }

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            local.EabReportSend.RptDetail = local.FileRecord.TextLine80;
            local.EabFileHandling.Action = "WRITE";
            UseCabControlReport1();

            if (!Equal(local.EabFileHandling.Status, "OK"))
            {
              ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

              return;
            }

            // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
            switch(TrimEnd(Substring(local.FileRecord.TextLine80, 1, 3)))
            {
              case "DPO":
                break;
              case "DPR":
                ++local.DistPolicyRule.Index;
                local.DistPolicyRule.CheckSize();

                // position 4 = debt state ("C"urrent or "A"rrears)
                // position 5 = apply to ("D"ebt or "I"nterest)
                // position 6 = distribute to order type code ("K"ansas,"I"
                // nterstate, or blank)
                // position 7 = debt function type ("A"ccruing or blank)
                local.DistPolicyRule.Update.Gr.ApplyTo =
                  Substring(local.FileRecord.TextLine80, 5, 1);
                local.DistPolicyRule.Update.Gr.CreatedBy = global.UserId;
                local.DistPolicyRule.Update.Gr.CreatedTmst = Now();
                local.DistPolicyRule.Update.Gr.DebtFunctionType =
                  Substring(local.FileRecord.TextLine80, 7, 1);
                local.DistPolicyRule.Update.Gr.DebtState =
                  Substring(local.FileRecord.TextLine80, 4, 1);
                local.DistPolicyRule.Update.Gr.DistributeToOrderTypeCode =
                  Substring(local.FileRecord.TextLine80, 6, 1);
                local.DistPolicyRule.Update.Gr.FirstLastIndicator = "";

                // --  Set the system generated id to max distribution policy 
                // rule system generated id + 1
                ++local.MaxDistributionPolicyRule.SystemGeneratedIdentifier;
                local.DistPolicyRule.Update.Gr.SystemGeneratedIdentifier =
                  local.MaxDistributionPolicyRule.SystemGeneratedIdentifier;
                local.DistPolicyRule.Item.Obligation.Index = -1;
                local.DistPolicyRule.Item.Obligation.Count = 0;
                local.DistPolicyRule.Item.DprProgram.Index = -1;
                local.DistPolicyRule.Item.DprProgram.Count = 0;

                break;
              case "OBT":
                local.J.Count = 0;

                // -- Loop through all the obligation types on the record...
                local.I.Count = 4;

                for(var limit = Length(TrimEnd(local.FileRecord.TextLine80)); local
                  .I.Count <= limit; ++local.I.Count)
                {
                  if (CharAt(local.FileRecord.TextLine80, local.I.Count) == ',')
                  {
                    ++local.DistPolicyRule.Item.Obligation.Index;
                    local.DistPolicyRule.Item.Obligation.CheckSize();

                    local.J.Count = 0;
                  }
                  else
                  {
                    if (local.I.Count == 4)
                    {
                      ++local.DistPolicyRule.Item.Obligation.Index;
                      local.DistPolicyRule.Item.Obligation.CheckSize();
                    }

                    if (local.J.Count == 0)
                    {
                      local.DistPolicyRule.Update.Obligation.Update.
                        GrObligationType.Code =
                          Substring(local.FileRecord.TextLine80, local.I.Count,
                        1);
                    }
                    else
                    {
                      local.DistPolicyRule.Update.Obligation.Update.
                        GrObligationType.Code =
                          Substring(local.DistPolicyRule.Item.Obligation.Item.
                          GrObligationType.Code, ObligationType.Code_MaxLength,
                        1, local.J.Count) + Substring
                        (local.FileRecord.TextLine80,
                        External.TextLine80_MaxLength, local.I.Count, 1);
                    }

                    ++local.J.Count;
                    local.DistPolicyRule.Update.Obligation.Update.
                      GrDprObligType.CreatedBy = global.UserId;
                    local.DistPolicyRule.Update.Obligation.Update.
                      GrDprObligType.CreatedTimestamp = Now();
                  }
                }

                break;
              case "PGM":
                local.J.Count = 0;

                // -- Loop through all the program types on the record...
                local.I.Count = 5;

                for(var limit = Length(TrimEnd(local.FileRecord.TextLine80)); local
                  .I.Count <= limit; ++local.I.Count)
                {
                  if (CharAt(local.FileRecord.TextLine80, local.I.Count) == ',')
                  {
                    ++local.DistPolicyRule.Item.DprProgram.Index;
                    local.DistPolicyRule.Item.DprProgram.CheckSize();

                    local.J.Count = 0;
                  }
                  else
                  {
                    if (local.I.Count == 5)
                    {
                      ++local.DistPolicyRule.Item.DprProgram.Index;
                      local.DistPolicyRule.Item.DprProgram.CheckSize();
                    }
                    else if (CharAt(local.FileRecord.TextLine80, local.I.Count) ==
                      '-')
                    {
                      local.DistPolicyRule.Update.DprProgram.Update.
                        GrDprProgram.ProgramState =
                          Substring(local.FileRecord.TextLine80, local.I.Count +
                        1, 2);
                      local.I.Count += 2;

                      continue;
                    }

                    if (local.J.Count == 0)
                    {
                      local.DistPolicyRule.Update.DprProgram.Update.GrProgram.
                        Code =
                          Substring(local.FileRecord.TextLine80, local.I.Count,
                        1);
                    }
                    else
                    {
                      local.DistPolicyRule.Update.DprProgram.Update.GrProgram.
                        Code =
                          Substring(local.DistPolicyRule.Item.DprProgram.Item.
                          GrProgram.Code, Program.Code_MaxLength, 1,
                        local.J.Count) + Substring
                        (local.FileRecord.TextLine80,
                        External.TextLine80_MaxLength, local.I.Count, 1);
                    }

                    ++local.J.Count;
                    local.DistPolicyRule.Update.DprProgram.Update.GrDprProgram.
                      AssistanceInd =
                        Substring(local.FileRecord.TextLine80, 4, 1);
                    local.DistPolicyRule.Update.DprProgram.Update.GrDprProgram.
                      CreatedBy = global.UserId;
                    local.DistPolicyRule.Update.DprProgram.Update.GrDprProgram.
                      CreatedTimestamp = Now();
                  }
                }

                break;
              default:
                break;
            }
          }
          while(!Equal(local.FileRecord.TextReturnCode, "EF") && !
            Equal(local.FileRecord.TextLine80, 1, 3, "DPO"));

          // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
          // Creation logic below...
          // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
          local.DistributionPolicy.Index = 0;

          for(var limit = local.DistributionPolicy.Count; local
            .DistributionPolicy.Index < limit; ++
            local.DistributionPolicy.Index)
          {
            if (!local.DistributionPolicy.CheckSize())
            {
              break;
            }

            // -- Read the collection type of the new distribution policy.
            if (!ReadCollectionType())
            {
              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
              local.EabReportSend.RptDetail = "Error #4";
              local.EabFileHandling.Action = "WRITE";
              UseCabControlReport1();

              if (!Equal(local.EabFileHandling.Status, "OK"))
              {
                ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                return;
              }

              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
              ExitState = "FN0000_COLLECTION_TYPE_NF_RB";
              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

              return;
            }

            try
            {
              CreateDistributionPolicy();
              ++local.NumDistPolicyInserts.Count;
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  local.EabReportSend.RptDetail = "Error #5";
                  local.EabFileHandling.Action = "WRITE";
                  UseCabControlReport1();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                    return;
                  }

                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.PermittedValueViolation:
                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  local.EabReportSend.RptDetail = "Error #6";
                  local.EabFileHandling.Action = "WRITE";
                  UseCabControlReport1();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                    return;
                  }

                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }

            local.DistPolicyRule.Index = 0;

            for(var limit1 = local.DistPolicyRule.Count; local
              .DistPolicyRule.Index < limit1; ++local.DistPolicyRule.Index)
            {
              if (!local.DistPolicyRule.CheckSize())
              {
                break;
              }

              try
              {
                CreateDistributionPolicyRule();
                ++local.NumDprInsert.Count;
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    local.EabReportSend.RptDetail = "Error #7";
                    local.EabFileHandling.Action = "WRITE";
                    UseCabControlReport1();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                      return;
                    }

                    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    local.EabReportSend.RptDetail = "Error #8";
                    local.EabFileHandling.Action = "WRITE";
                    UseCabControlReport1();

                    if (!Equal(local.EabFileHandling.Status, "OK"))
                    {
                      ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                      return;
                    }

                    // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                    ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              local.DistPolicyRule.Item.Obligation.Index = 0;

              for(var limit2 = local.DistPolicyRule.Item.Obligation.Count; local
                .DistPolicyRule.Item.Obligation.Index < limit2; ++
                local.DistPolicyRule.Item.Obligation.Index)
              {
                if (!local.DistPolicyRule.Item.Obligation.CheckSize())
                {
                  break;
                }

                if (!ReadObligationType())
                {
                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  local.EabReportSend.RptDetail = "Error #9";
                  local.EabFileHandling.Action = "WRITE";
                  UseCabControlReport1();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                    return;
                  }

                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                try
                {
                  CreateDprObligType();
                  ++local.NumDprObTypeInserts.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      local.EabReportSend.RptDetail = "Error #10";
                      local.EabFileHandling.Action = "WRITE";
                      UseCabControlReport1();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                        return;
                      }

                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      local.EabReportSend.RptDetail = "Error #11";
                      local.EabFileHandling.Action = "WRITE";
                      UseCabControlReport1();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                        return;
                      }

                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              local.DistPolicyRule.Item.Obligation.CheckIndex();
              local.DistPolicyRule.Item.DprProgram.Index = 0;

              for(var limit2 = local.DistPolicyRule.Item.DprProgram.Count; local
                .DistPolicyRule.Item.DprProgram.Index < limit2; ++
                local.DistPolicyRule.Item.DprProgram.Index)
              {
                if (!local.DistPolicyRule.Item.DprProgram.CheckSize())
                {
                  break;
                }

                if (!ReadProgram())
                {
                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  local.EabReportSend.RptDetail = "Error #12";
                  local.EabFileHandling.Action = "WRITE";
                  UseCabControlReport1();

                  if (!Equal(local.EabFileHandling.Status, "OK"))
                  {
                    ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                    return;
                  }

                  // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                  ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                  return;
                }

                try
                {
                  CreateDprProgram();
                  ++local.NumDprProgramInserts.Count;
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      if (ReadDprProgram())
                      {
                        try
                        {
                          UpdateDprProgram();
                        }
                        catch(Exception e1)
                        {
                          switch(GetErrorCode(e1))
                          {
                            case ErrorCode.AlreadyExists:
                              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                              local.EabReportSend.RptDetail = "Error #13";
                              local.EabFileHandling.Action = "WRITE";
                              UseCabControlReport1();

                              if (!Equal(local.EabFileHandling.Status, "OK"))
                              {
                                ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                                return;
                              }

                              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                              return;
                            case ErrorCode.PermittedValueViolation:
                              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                              local.EabReportSend.RptDetail = "Error #14";
                              local.EabFileHandling.Action = "WRITE";
                              UseCabControlReport1();

                              if (!Equal(local.EabFileHandling.Status, "OK"))
                              {
                                ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                                return;
                              }

                              // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                              ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

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
                        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                        local.EabReportSend.RptDetail = "Error #15";
                        local.EabFileHandling.Action = "WRITE";
                        UseCabControlReport1();

                        if (!Equal(local.EabFileHandling.Status, "OK"))
                        {
                          ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                          return;
                        }

                        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                        ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                        return;
                      }

                      break;
                    case ErrorCode.PermittedValueViolation:
                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      local.EabReportSend.RptDetail = "Error #16";
                      local.EabFileHandling.Action = "WRITE";
                      UseCabControlReport1();

                      if (!Equal(local.EabFileHandling.Status, "OK"))
                      {
                        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

                        return;
                      }

                      // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }
              }

              local.DistPolicyRule.Item.DprProgram.CheckIndex();
            }

            local.DistPolicyRule.CheckIndex();
          }

          local.DistributionPolicy.CheckIndex();
          local.DoNothing.Flag = "X";

          break;
        default:
          // -- Read next record...
          local.FileRecord.Assign(local.Null1);
          local.FileRecord.FileInstruction = "READ";
          UseFnB721ExtReadFile();

          if (!Equal(local.FileRecord.TextReturnCode, "OK") && !
            Equal(local.FileRecord.TextReturnCode, "EF"))
          {
            ExitState = "OE0000_ERROR_READING_EXT_FILE";

            return;
          }

          // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
          local.EabReportSend.RptDetail = local.FileRecord.TextLine80;
          local.EabFileHandling.Action = "WRITE";
          UseCabControlReport1();

          if (!Equal(local.EabFileHandling.Status, "OK"))
          {
            ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

            return;
          }

          // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
          break;
      }

      local.DoNothing.Flag = "X";
    }
    while(!Equal(local.FileRecord.TextReturnCode, "EF"));

    for(local.I.Count = 1; local.I.Count <= 9; ++local.I.Count)
    {
      switch(local.I.Count)
      {
        case 5:
          local.EabReportSend.RptDetail =
            "Number of Old Distribution Policy rows updated . . . . . ." + NumberToString
            (local.NumDistPolicyUpdates.Count, 15);

          break;
        case 6:
          local.EabReportSend.RptDetail =
            "Number of New Distribution Policy rows inserted. . . . . ." + NumberToString
            (local.NumDistPolicyInserts.Count, 15);

          break;
        case 7:
          local.EabReportSend.RptDetail =
            "Number of New Distribution Policy Rule rows inserted . . ." + NumberToString
            (local.NumDprInsert.Count, 15);

          break;
        case 8:
          local.EabReportSend.RptDetail =
            "Number of New DPR Obligation Type rows inserted. . . . . ." + NumberToString
            (local.NumDprObTypeInserts.Count, 15);

          break;
        case 9:
          local.EabReportSend.RptDetail =
            "Number of New DPR Program rows inserted. . . . . . . . . ." + NumberToString
            (local.NumDprProgramInserts.Count, 15);

          break;
        default:
          local.EabReportSend.RptDetail = "";

          break;
      }

      local.EabFileHandling.Action = "WRITE";
      UseCabControlReport1();

      if (!Equal(local.EabFileHandling.Status, "OK"))
      {
        ExitState = "FN0000_ERROR_WRITING_CONTROL_RPT";

        return;
      }
    }

    // -- Close the distribution policy rule file
    local.FileRecord.Assign(local.Null1);
    local.FileRecord.FileInstruction = "CLOSE";
    UseFnB721ExtReadFile();

    if (!Equal(local.FileRecord.TextReturnCode, "OK"))
    {
      ExitState = "OE0000_ERROR_CLOSING_EXTERNAL";

      return;
    }

    // -- Close the control report.
    local.EabFileHandling.Action = "CLOSE";
    UseCabControlReport3();
  }

  private static void MoveDistPolicyRule(FnBfxkInitGroups.Export.
    DistPolicyRuleGroup source, Local.DistPolicyRuleGroup target)
  {
    target.Gr.Assign(source.Gr);
    source.Obligation.CopyTo(target.Obligation, MoveObligation);
    source.DprProgram.CopyTo(target.DprProgram, MoveDprProgram);
  }

  private static void MoveDistributionPolicy(FnBfxkInitGroups.Export.
    DistributionPolicyGroup source, Local.DistributionPolicyGroup target)
  {
    target.GrCollectionType.Code = source.GrCollectionType.Code;
    target.GrDistributionPolicy.Assign(source.GrDistributionPolicy);
  }

  private static void MoveDprObligType(DprObligType source, DprObligType target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveDprProgram(FnBfxkInitGroups.Export.
    DprProgramGroup source, Local.DprProgramGroup target)
  {
    target.GrProgram.Code = source.GrProgram.Code;
    target.GrDprProgram.Assign(source.GrDprProgram);
  }

  private static void MoveEabReportSend(EabReportSend source,
    EabReportSend target)
  {
    target.ProcessDate = source.ProcessDate;
    target.ProgramName = source.ProgramName;
  }

  private static void MoveExternal(External source, External target)
  {
    target.TextReturnCode = source.TextReturnCode;
    target.TextLine80 = source.TextLine80;
  }

  private static void MoveObligation(FnBfxkInitGroups.Export.
    ObligationGroup source, Local.ObligationGroup target)
  {
    target.GrObligationType.Code = source.GrObligationType.Code;
    MoveDprObligType(source.GrDprObligType, target.GrDprObligType);
  }

  private void UseCabControlReport1()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.NeededToWrite.RptDetail = local.EabReportSend.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport2()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    MoveEabReportSend(local.EabReportSend, useImport.NeededToOpen);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabControlReport3()
  {
    var useImport = new CabControlReport.Import();
    var useExport = new CabControlReport.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabControlReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseFnB721ExtReadFile()
  {
    var useImport = new FnB721ExtReadFile.Import();
    var useExport = new FnB721ExtReadFile.Export();

    useImport.FileNumber.Count = local.FileNumber.Count;
    useImport.External.FileInstruction = local.FileRecord.FileInstruction;
    MoveExternal(local.FileRecord, useExport.External);

    Call(FnB721ExtReadFile.Execute, useImport, useExport);

    MoveExternal(useExport.External, local.FileRecord);
  }

  private void UseFnBfxkInitGroups()
  {
    var useImport = new FnBfxkInitGroups.Import();
    var useExport = new FnBfxkInitGroups.Export();

    Call(FnBfxkInitGroups.Execute, useImport, useExport);

    useExport.DistributionPolicy.CopyTo(
      local.DistributionPolicy, MoveDistributionPolicy);
    useExport.DistPolicyRule.CopyTo(local.DistPolicyRule, MoveDistPolicyRule);
  }

  private void CreateDistributionPolicy()
  {
    var systemGeneratedIdentifier =
      local.DistributionPolicy.Item.GrDistributionPolicy.
        SystemGeneratedIdentifier;
    var name = local.DistributionPolicy.Item.GrDistributionPolicy.Name;
    var effectiveDt =
      local.DistributionPolicy.Item.GrDistributionPolicy.EffectiveDt;
    var discontinueDt =
      local.DistributionPolicy.Item.GrDistributionPolicy.DiscontinueDt;
    var maximumProcessedDt =
      local.DistributionPolicy.Item.GrDistributionPolicy.MaximumProcessedDt;
    var createdBy =
      local.DistributionPolicy.Item.GrDistributionPolicy.CreatedBy;
    var createdTmst =
      local.DistributionPolicy.Item.GrDistributionPolicy.CreatedTmst;
    var lastUpdatedBy =
      local.DistributionPolicy.Item.GrDistributionPolicy.LastUpdatedBy ?? "";
    var lastUpdatedTmst =
      local.DistributionPolicy.Item.GrDistributionPolicy.LastUpdatedTmst;
    var cltIdentifier = entities.CollectionType.SequentialIdentifier;
    var description =
      local.DistributionPolicy.Item.GrDistributionPolicy.Description;

    entities.DistributionPolicy1.Populated = false;
    Update("CreateDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(command, "distPlcyId", systemGeneratedIdentifier);
        db.SetString(command, "distPlcyNm", name);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableDate(command, "maxPrcdDt", maximumProcessedDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableInt32(command, "cltIdentifier", cltIdentifier);
        db.SetString(command, "distPlcyDsc", description);
      });

    entities.DistributionPolicy1.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DistributionPolicy1.Name = name;
    entities.DistributionPolicy1.EffectiveDt = effectiveDt;
    entities.DistributionPolicy1.DiscontinueDt = discontinueDt;
    entities.DistributionPolicy1.MaximumProcessedDt = maximumProcessedDt;
    entities.DistributionPolicy1.CreatedBy = createdBy;
    entities.DistributionPolicy1.CreatedTmst = createdTmst;
    entities.DistributionPolicy1.LastUpdatedBy = lastUpdatedBy;
    entities.DistributionPolicy1.LastUpdatedTmst = lastUpdatedTmst;
    entities.DistributionPolicy1.CltIdentifier = cltIdentifier;
    entities.DistributionPolicy1.Description = description;
    entities.DistributionPolicy1.Populated = true;
  }

  private void CreateDistributionPolicyRule()
  {
    var dbpGeneratedId = entities.DistributionPolicy1.SystemGeneratedIdentifier;
    var systemGeneratedIdentifier =
      local.DistPolicyRule.Item.Gr.SystemGeneratedIdentifier;
    var firstLastIndicator =
      local.DistPolicyRule.Item.Gr.FirstLastIndicator ?? "";
    var debtFunctionType = local.DistPolicyRule.Item.Gr.DebtFunctionType;
    var debtState = local.DistPolicyRule.Item.Gr.DebtState;
    var applyTo = local.DistPolicyRule.Item.Gr.ApplyTo;
    var createdBy = local.DistPolicyRule.Item.Gr.CreatedBy;
    var createdTmst = local.DistPolicyRule.Item.Gr.CreatedTmst;
    var lastUpdatedBy = local.DistPolicyRule.Item.Gr.LastUpdatedBy ?? "";
    var lastUpdatedTmst = local.DistPolicyRule.Item.Gr.LastUpdatedTmst;
    var distributeToOrderTypeCode =
      local.DistPolicyRule.Item.Gr.DistributeToOrderTypeCode;

    CheckValid<DistributionPolicyRule>("FirstLastIndicator", firstLastIndicator);
      
    CheckValid<DistributionPolicyRule>("DebtFunctionType", debtFunctionType);
    CheckValid<DistributionPolicyRule>("DebtState", debtState);
    CheckValid<DistributionPolicyRule>("ApplyTo", applyTo);
    CheckValid<DistributionPolicyRule>("DistributeToOrderTypeCode",
      distributeToOrderTypeCode);
    entities.DistributionPolicyRule.Populated = false;
    Update("CreateDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "distPlcyRlId", systemGeneratedIdentifier);
        db.SetNullableString(command, "firstLastInd", firstLastIndicator);
        db.SetString(command, "debtFuncTyp", debtFunctionType);
        db.SetString(command, "debtState", debtState);
        db.SetString(command, "applyTo", applyTo);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "distToOrdTypCd", distributeToOrderTypeCode);
      });

    entities.DistributionPolicyRule.DbpGeneratedId = dbpGeneratedId;
    entities.DistributionPolicyRule.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DistributionPolicyRule.FirstLastIndicator = firstLastIndicator;
    entities.DistributionPolicyRule.DebtFunctionType = debtFunctionType;
    entities.DistributionPolicyRule.DebtState = debtState;
    entities.DistributionPolicyRule.ApplyTo = applyTo;
    entities.DistributionPolicyRule.CreatedBy = createdBy;
    entities.DistributionPolicyRule.CreatedTmst = createdTmst;
    entities.DistributionPolicyRule.LastUpdatedBy = lastUpdatedBy;
    entities.DistributionPolicyRule.LastUpdatedTmst = lastUpdatedTmst;
    entities.DistributionPolicyRule.DprNextId = null;
    entities.DistributionPolicyRule.DistributeToOrderTypeCode =
      distributeToOrderTypeCode;
    entities.DistributionPolicyRule.Populated = true;
  }

  private void CreateDprObligType()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);

    var createdBy =
      local.DistPolicyRule.Item.Obligation.Item.GrDprObligType.CreatedBy;
    var createdTimestamp =
      local.DistPolicyRule.Item.Obligation.Item.GrDprObligType.CreatedTimestamp;
      
    var otyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var dbpGeneratedId = entities.DistributionPolicyRule.DbpGeneratedId;
    var dprGeneratedId =
      entities.DistributionPolicyRule.SystemGeneratedIdentifier;

    entities.DprObligType.Populated = false;
    Update("CreateDprObligType",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetInt32(command, "otyGeneratedId", otyGeneratedId);
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "dprGeneratedId", dprGeneratedId);
      });

    entities.DprObligType.CreatedBy = createdBy;
    entities.DprObligType.CreatedTimestamp = createdTimestamp;
    entities.DprObligType.OtyGeneratedId = otyGeneratedId;
    entities.DprObligType.DbpGeneratedId = dbpGeneratedId;
    entities.DprObligType.DprGeneratedId = dprGeneratedId;
    entities.DprObligType.Populated = true;
  }

  private void CreateDprProgram()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);

    var createdBy =
      local.DistPolicyRule.Item.DprProgram.Item.GrDprProgram.CreatedBy;
    var createdTimestamp =
      local.DistPolicyRule.Item.DprProgram.Item.GrDprProgram.CreatedTimestamp;
    var dbpGeneratedId = entities.DistributionPolicyRule.DbpGeneratedId;
    var dprGeneratedId =
      entities.DistributionPolicyRule.SystemGeneratedIdentifier;
    var prgGeneratedId = entities.Program.SystemGeneratedIdentifier;
    var programState =
      local.DistPolicyRule.Item.DprProgram.Item.GrDprProgram.ProgramState;
    var assistanceInd =
      local.DistPolicyRule.Item.DprProgram.Item.GrDprProgram.AssistanceInd ?? ""
      ;

    entities.DprProgram1.Populated = false;
    Update("CreateDprProgram",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "dprGeneratedId", dprGeneratedId);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetString(command, "programState", programState);
        db.SetNullableString(command, "assistanceInd", assistanceInd);
      });

    entities.DprProgram1.CreatedBy = createdBy;
    entities.DprProgram1.CreatedTimestamp = createdTimestamp;
    entities.DprProgram1.DbpGeneratedId = dbpGeneratedId;
    entities.DprProgram1.DprGeneratedId = dprGeneratedId;
    entities.DprProgram1.PrgGeneratedId = prgGeneratedId;
    entities.DprProgram1.ProgramState = programState;
    entities.DprProgram1.AssistanceInd = assistanceInd;
    entities.DprProgram1.Populated = true;
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      (db, command) =>
      {
        db.SetString(
          command, "code", local.DistributionPolicy.Item.GrCollectionType.Code);
          
        db.SetDate(
          command, "effectiveDate",
          local.Dist2009StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.EffectiveDate = db.GetDate(reader, 2);
        entities.CollectionType.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.CollectionType.Populated = true;
      });
  }

  private bool ReadDistributionPolicy1()
  {
    entities.DistributionPolicy1.Populated = false;

    return Read("ReadDistributionPolicy1",
      null,
      (db, reader) =>
      {
        entities.DistributionPolicy1.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy1.Name = db.GetString(reader, 1);
        entities.DistributionPolicy1.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy1.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy1.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy1.CreatedBy = db.GetString(reader, 5);
        entities.DistributionPolicy1.CreatedTmst = db.GetDateTime(reader, 6);
        entities.DistributionPolicy1.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.DistributionPolicy1.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.DistributionPolicy1.CltIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.DistributionPolicy1.Description = db.GetString(reader, 10);
        entities.DistributionPolicy1.Populated = true;
      });
  }

  private bool ReadDistributionPolicy2()
  {
    entities.Old.Populated = false;

    return Read("ReadDistributionPolicy2",
      (db, command) =>
      {
        db.SetString(
          command, "code", local.DistributionPolicy.Item.GrCollectionType.Code);
          
      },
      (db, reader) =>
      {
        entities.Old.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Old.Name = db.GetString(reader, 1);
        entities.Old.EffectiveDt = db.GetDate(reader, 2);
        entities.Old.DiscontinueDt = db.GetNullableDate(reader, 3);
        entities.Old.MaximumProcessedDt = db.GetNullableDate(reader, 4);
        entities.Old.CreatedBy = db.GetString(reader, 5);
        entities.Old.CreatedTmst = db.GetDateTime(reader, 6);
        entities.Old.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Old.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Old.CltIdentifier = db.GetNullableInt32(reader, 9);
        entities.Old.Description = db.GetString(reader, 10);
        entities.Old.Populated = true;
      });
  }

  private bool ReadDprProgram()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);
    entities.DprProgram1.Populated = false;

    return Read("ReadDprProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicyRule.DbpGeneratedId);
        db.SetInt32(
          command, "prgGeneratedId",
          entities.Program.SystemGeneratedIdentifier);
        db.SetString(
          command, "programState",
          local.DistPolicyRule.Item.DprProgram.Item.GrDprProgram.ProgramState);
      },
      (db, reader) =>
      {
        entities.DprProgram1.CreatedBy = db.GetString(reader, 0);
        entities.DprProgram1.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.DprProgram1.DbpGeneratedId = db.GetInt32(reader, 2);
        entities.DprProgram1.DprGeneratedId = db.GetInt32(reader, 3);
        entities.DprProgram1.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.DprProgram1.ProgramState = db.GetString(reader, 5);
        entities.DprProgram1.AssistanceInd = db.GetNullableString(reader, 6);
        entities.DprProgram1.Populated = true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(
          command, "debtTypCd",
          local.DistPolicyRule.Item.Obligation.Item.GrObligationType.Code);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 2);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 3);
        entities.ObligationType.Populated = true;
      });
  }

  private bool ReadProgram()
  {
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetString(
          command, "code",
          local.DistPolicyRule.Item.DprProgram.Item.GrProgram.Code);
        db.SetDate(
          command, "effectiveDate",
          local.Dist2009StartDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Code = db.GetString(reader, 1);
        entities.Program.EffectiveDate = db.GetDate(reader, 2);
        entities.Program.DiscontinueDate = db.GetDate(reader, 3);
        entities.Program.Populated = true;
      });
  }

  private void UpdateDistributionPolicy()
  {
    var discontinueDt = local.Dist2009StartDate.Date;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.Old.Populated = false;
    Update("UpdateDistributionPolicy",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "distPlcyId", entities.Old.SystemGeneratedIdentifier);
      });

    entities.Old.DiscontinueDt = discontinueDt;
    entities.Old.LastUpdatedBy = lastUpdatedBy;
    entities.Old.LastUpdatedTmst = lastUpdatedTmst;
    entities.Old.Populated = true;
  }

  private void UpdateDprProgram()
  {
    System.Diagnostics.Debug.Assert(entities.DprProgram1.Populated);
    entities.DprProgram1.Populated = false;
    Update("UpdateDprProgram",
      (db, command) =>
      {
        db.SetNullableString(command, "assistanceInd", "");
        db.SetInt32(
          command, "dbpGeneratedId", entities.DprProgram1.DbpGeneratedId);
        db.SetInt32(
          command, "dprGeneratedId", entities.DprProgram1.DprGeneratedId);
        db.SetInt32(
          command, "prgGeneratedId", entities.DprProgram1.PrgGeneratedId);
        db.
          SetString(command, "programState", entities.DprProgram1.ProgramState);
          
      });

    entities.DprProgram1.AssistanceInd = "";
    entities.DprProgram1.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A DistributionPolicyGroup group.</summary>
    [Serializable]
    public class DistributionPolicyGroup
    {
      /// <summary>
      /// A value of GrCollectionType.
      /// </summary>
      [JsonPropertyName("grCollectionType")]
      public CollectionType GrCollectionType
      {
        get => grCollectionType ??= new();
        set => grCollectionType = value;
      }

      /// <summary>
      /// A value of GrDistributionPolicy.
      /// </summary>
      [JsonPropertyName("grDistributionPolicy")]
      public DistributionPolicy GrDistributionPolicy
      {
        get => grDistributionPolicy ??= new();
        set => grDistributionPolicy = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private CollectionType grCollectionType;
      private DistributionPolicy grDistributionPolicy;
    }

    /// <summary>A DistPolicyRuleGroup group.</summary>
    [Serializable]
    public class DistPolicyRuleGroup
    {
      /// <summary>
      /// A value of Gr.
      /// </summary>
      [JsonPropertyName("gr")]
      public DistributionPolicyRule Gr
      {
        get => gr ??= new();
        set => gr = value;
      }

      /// <summary>
      /// Gets a value of Obligation.
      /// </summary>
      [JsonIgnore]
      public Array<ObligationGroup> Obligation => obligation ??= new(
        ObligationGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of Obligation for json serialization.
      /// </summary>
      [JsonPropertyName("obligation")]
      [Computed]
      public IList<ObligationGroup> Obligation_Json
      {
        get => obligation;
        set => Obligation.Assign(value);
      }

      /// <summary>
      /// Gets a value of DprProgram.
      /// </summary>
      [JsonIgnore]
      public Array<DprProgramGroup> DprProgram => dprProgram ??= new(
        DprProgramGroup.Capacity, 0);

      /// <summary>
      /// Gets a value of DprProgram for json serialization.
      /// </summary>
      [JsonPropertyName("dprProgram")]
      [Computed]
      public IList<DprProgramGroup> DprProgram_Json
      {
        get => dprProgram;
        set => DprProgram.Assign(value);
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private DistributionPolicyRule gr;
      private Array<ObligationGroup> obligation;
      private Array<DprProgramGroup> dprProgram;
    }

    /// <summary>A ObligationGroup group.</summary>
    [Serializable]
    public class ObligationGroup
    {
      /// <summary>
      /// A value of GrObligationType.
      /// </summary>
      [JsonPropertyName("grObligationType")]
      public ObligationType GrObligationType
      {
        get => grObligationType ??= new();
        set => grObligationType = value;
      }

      /// <summary>
      /// A value of GrDprObligType.
      /// </summary>
      [JsonPropertyName("grDprObligType")]
      public DprObligType GrDprObligType
      {
        get => grDprObligType ??= new();
        set => grDprObligType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private ObligationType grObligationType;
      private DprObligType grDprObligType;
    }

    /// <summary>A DprProgramGroup group.</summary>
    [Serializable]
    public class DprProgramGroup
    {
      /// <summary>
      /// A value of GrProgram.
      /// </summary>
      [JsonPropertyName("grProgram")]
      public Program GrProgram
      {
        get => grProgram ??= new();
        set => grProgram = value;
      }

      /// <summary>
      /// A value of GrDprProgram.
      /// </summary>
      [JsonPropertyName("grDprProgram")]
      public DprProgram GrDprProgram
      {
        get => grDprProgram ??= new();
        set => grDprProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Program grProgram;
      private DprProgram grDprProgram;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of NumDprProgramInserts.
    /// </summary>
    [JsonPropertyName("numDprProgramInserts")]
    public Common NumDprProgramInserts
    {
      get => numDprProgramInserts ??= new();
      set => numDprProgramInserts = value;
    }

    /// <summary>
    /// A value of NumDprObTypeInserts.
    /// </summary>
    [JsonPropertyName("numDprObTypeInserts")]
    public Common NumDprObTypeInserts
    {
      get => numDprObTypeInserts ??= new();
      set => numDprObTypeInserts = value;
    }

    /// <summary>
    /// A value of NumDprInsert.
    /// </summary>
    [JsonPropertyName("numDprInsert")]
    public Common NumDprInsert
    {
      get => numDprInsert ??= new();
      set => numDprInsert = value;
    }

    /// <summary>
    /// A value of NumDistPolicyInserts.
    /// </summary>
    [JsonPropertyName("numDistPolicyInserts")]
    public Common NumDistPolicyInserts
    {
      get => numDistPolicyInserts ??= new();
      set => numDistPolicyInserts = value;
    }

    /// <summary>
    /// A value of NumDistPolicyUpdates.
    /// </summary>
    [JsonPropertyName("numDistPolicyUpdates")]
    public Common NumDistPolicyUpdates
    {
      get => numDistPolicyUpdates ??= new();
      set => numDistPolicyUpdates = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public External Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of DoNothing.
    /// </summary>
    [JsonPropertyName("doNothing")]
    public Common DoNothing
    {
      get => doNothing ??= new();
      set => doNothing = value;
    }

    /// <summary>
    /// A value of J.
    /// </summary>
    [JsonPropertyName("j")]
    public Common J
    {
      get => j ??= new();
      set => j = value;
    }

    /// <summary>
    /// A value of FileNumber.
    /// </summary>
    [JsonPropertyName("fileNumber")]
    public Common FileNumber
    {
      get => fileNumber ??= new();
      set => fileNumber = value;
    }

    /// <summary>
    /// Gets a value of DistributionPolicy.
    /// </summary>
    [JsonIgnore]
    public Array<DistributionPolicyGroup> DistributionPolicy =>
      distributionPolicy ??= new(DistributionPolicyGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DistributionPolicy for json serialization.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    [Computed]
    public IList<DistributionPolicyGroup> DistributionPolicy_Json
    {
      get => distributionPolicy;
      set => DistributionPolicy.Assign(value);
    }

    /// <summary>
    /// Gets a value of DistPolicyRule.
    /// </summary>
    [JsonIgnore]
    public Array<DistPolicyRuleGroup> DistPolicyRule => distPolicyRule ??= new(
      DistPolicyRuleGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of DistPolicyRule for json serialization.
    /// </summary>
    [JsonPropertyName("distPolicyRule")]
    [Computed]
    public IList<DistPolicyRuleGroup> DistPolicyRule_Json
    {
      get => distPolicyRule;
      set => DistPolicyRule.Assign(value);
    }

    /// <summary>
    /// A value of I.
    /// </summary>
    [JsonPropertyName("i")]
    public Common I
    {
      get => i ??= new();
      set => i = value;
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
    /// A value of FileRecord.
    /// </summary>
    [JsonPropertyName("fileRecord")]
    public External FileRecord
    {
      get => fileRecord ??= new();
      set => fileRecord = value;
    }

    /// <summary>
    /// A value of MaxDistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("maxDistributionPolicyRule")]
    public DistributionPolicyRule MaxDistributionPolicyRule
    {
      get => maxDistributionPolicyRule ??= new();
      set => maxDistributionPolicyRule = value;
    }

    /// <summary>
    /// A value of MaxDistributionPolicy.
    /// </summary>
    [JsonPropertyName("maxDistributionPolicy")]
    public DistributionPolicy MaxDistributionPolicy
    {
      get => maxDistributionPolicy ??= new();
      set => maxDistributionPolicy = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of Dist2009StartDate.
    /// </summary>
    [JsonPropertyName("dist2009StartDate")]
    public DateWorkArea Dist2009StartDate
    {
      get => dist2009StartDate ??= new();
      set => dist2009StartDate = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
    private Common numDprProgramInserts;
    private Common numDprObTypeInserts;
    private Common numDprInsert;
    private Common numDistPolicyInserts;
    private Common numDistPolicyUpdates;
    private External null1;
    private Common doNothing;
    private Common j;
    private Common fileNumber;
    private Array<DistributionPolicyGroup> distributionPolicy;
    private Array<DistPolicyRuleGroup> distPolicyRule;
    private Common i;
    private ObligationType obligationType;
    private External fileRecord;
    private DistributionPolicyRule maxDistributionPolicyRule;
    private DistributionPolicy maxDistributionPolicy;
    private DateWorkArea maxDate;
    private DateWorkArea dist2009StartDate;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public DistributionPolicy Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of DistributionPolicy1.
    /// </summary>
    [JsonPropertyName("distributionPolicy1")]
    public DistributionPolicy DistributionPolicy1
    {
      get => distributionPolicy1 ??= new();
      set => distributionPolicy1 = value;
    }

    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    /// <summary>
    /// A value of DprObligType.
    /// </summary>
    [JsonPropertyName("dprObligType")]
    public DprObligType DprObligType
    {
      get => dprObligType ??= new();
      set => dprObligType = value;
    }

    /// <summary>
    /// A value of DprProgram1.
    /// </summary>
    [JsonPropertyName("dprProgram1")]
    public DprProgram DprProgram1
    {
      get => dprProgram1 ??= new();
      set => dprProgram1 = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    private DistributionPolicy old;
    private DistributionPolicy distributionPolicy1;
    private DistributionPolicyRule distributionPolicyRule;
    private DprObligType dprObligType;
    private DprProgram dprProgram1;
    private Program program;
    private ObligationType obligationType;
    private CollectionType collectionType;
  }
#endregion
}
