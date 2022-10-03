// Program: FN_B700_OCSE34_STEP_6, ID: 373315418, model: 746.
// Short name: SWE02991
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B700_OCSE34_STEP_6.
/// </summary>
[Serializable]
public partial class FnB700Ocse34Step6: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B700_OCSE34_STEP_6 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB700Ocse34Step6(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB700Ocse34Step6.
  /// </summary>
  public FnB700Ocse34Step6(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ****************************************************************************
    // **                 M A I N T E N A N C E   L O G
    // ****************************************************************************
    // ** Date		WR/PR	Developer	Description
    // ****************************************************************************
    // ** 12/05/2003	040134	E.Shirk		Federally mandated OCSE34 report changes.
    // ** 02/28/2005	040796	E.Shirk		Fed mandated changes for new undistributed 
    // collections report.
    // 9/15/2005  pr 244780  subtract out the fdso total which will make line 9d
    // match line 7 of natural report
    // ** 12/03/2007	CQ295	GVandy		Federally mandated changes to OCSE34 report.
    // ** 01/11/2009	CQ14811	GVandy		Non-IVD Stale, Held, and Re-issue amounts 
    // are being overridden.
    // ***************************************************************************
    if (AsChar(import.ProgramCheckpointRestart.RestartInd) == 'Y' && Equal
      (import.ProgramCheckpointRestart.RestartInfo, 1, 2, "06"))
    {
      UseFnB700BuildGvForRestart();
    }

    if (import.Group.IsEmpty)
    {
      // -- The import group is empty if the PPI record is set to begin 
      // processing at this step.
      // --  We need to load the values calculated in the previous steps so that
      // they are not overridden with zeros.
      UseFnB700BuildGvForRestart();
    }

    ReadOcse34();

    import.Group.Index = 0;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency =
      entities.Ocse34.NetUndistributedAmount.GetValueOrDefault();

    // ***************************************************************************
    // **       Calculate line 2 Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 2; import.Group.Index < 9; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    // -- Include new line 2g (Collections from other countries) in the Line 2 
    // Column G total.
    import.Group.Index = 63;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 1;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 4a Column G.
    // ***************************************************************************
    import.Group.Index = 10;
    import.Group.CheckSize();

    // ** 01/11/2009  CQ14811  GVandy  Non-IVD Stale, Held, and Re-issue amounts
    // are being overridden.
    import.Group.Update.Common.TotalCurrency =
      import.Ocse34.KpcNonIvdIwoForwCollAmt.GetValueOrDefault() + import
      .Group.Item.Common.TotalCurrency;

    // ***************************************************************************
    // **      Calculate line 4b Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 11; import.Group.Index < 15; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    import.Group.Index = 50;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 51;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 15;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 4 Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 10;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 15;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 65;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 64;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate Line 6 Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 0;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 1;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 9;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 10;
    import.Group.CheckSize();

    local.Amount.TotalCurrency -= import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 15;
    import.Group.CheckSize();

    local.Amount.TotalCurrency -= import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 65;
    import.Group.CheckSize();

    local.Amount.TotalCurrency -= import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 16;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate Line 7a Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 66; import.Group.Index < 70; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    import.Group.Index = 70;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate Line 7b Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 17; import.Group.Index < 20; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    import.Group.Index = 52;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 20;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate Line 7c Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 21; import.Group.Index < 25; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    import.Group.Index = 53;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 54;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 25;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate Line 7d Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 26; import.Group.Index < 30; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    import.Group.Index = 55;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 56;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 30;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate Line 7e Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 71; import.Group.Index < 73; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    import.Group.Index = 73;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 8 Column A.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 17;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 21;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 26;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 31;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 8 Column B.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 18;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 22;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 27;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 32;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 8 Column C.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 19;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 23;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 28;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 33;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 8 Column D.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 52;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 53;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 55;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 57;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 8 Column E.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 24;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 29;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 34;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 8 Column F.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 54;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 56;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 58;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 8 Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    for(import.Group.Index = 31; import.Group.Index < 35; ++import.Group.Index)
    {
      if (!import.Group.CheckSize())
      {
        break;
      }

      local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;
    }

    import.Group.CheckIndex();

    import.Group.Index = 57;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 58;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 35;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 9 Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 16;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 35;
    import.Group.CheckSize();

    local.Amount.TotalCurrency -= import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 36;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 9B Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 36;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 37;
    import.Group.CheckSize();

    local.Amount.TotalCurrency -= import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 38;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Perform line 9 Column G out of balance check.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 37;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 38;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 36;
    import.Group.CheckSize();

    if (import.Group.Item.Common.TotalCurrency == local.Amount.TotalCurrency)
    {
      export.Line9OobInd.Flag = "N";
    }
    else
    {
      export.Line9OobInd.Flag = "Y";
    }

    // ***************************************************************************
    // **       Calculate line 9C Column G.
    // ***************************************************************************
    import.Group.Index = 59;
    import.Group.CheckSize();

    local.Lda.TotalCurrency = import.Group.Item.Common.TotalCurrency + import
      .Ocse34.KpcNivdIwoLda.GetValueOrDefault();
    import.Group.Update.Common.TotalCurrency =
      import.Group.Item.Common.TotalCurrency + import
      .Ocse34.KpcNivdIwoLda.GetValueOrDefault() + import
      .Ocse34.FdsoDsbSuppAmt.GetValueOrDefault();

    // ***************************************************************************
    // **       Calculate line 9D Column G.
    // ***************************************************************************
    // 9/15/2005 changed the calculation to subtract out fdso - pr244780 and 
    // legal and future
    local.Amount.TotalCurrency = 0;
    local.Amount.TotalCurrency =
      (long)import.Ocse34.KpcHeldDisbAmt.GetValueOrDefault() + import
      .Ocse34.KpcStaleDateAmt.GetValueOrDefault() + import
      .Ocse34.KpcUiNonIvdIwoAmt.GetValueOrDefault() + import
      .Ocse34.CseCshRcptDtlSuspAmt.GetValueOrDefault() + import
      .Ocse34.CseDisbCreditAmt.GetValueOrDefault() + import
      .Ocse34.CseDisbDebitAmt.GetValueOrDefault() + import
      .Ocse34.CseWarrantAmt.GetValueOrDefault() + import
      .Ocse34.CseInterstateAmt.GetValueOrDefault() + import
      .Ocse34.CseDisbSuppressAmt.GetValueOrDefault() + import
      .Ocse34.CsePaymentAmt.GetValueOrDefault() - (
        decimal)import.Ocse34.FdsoDsbSuppAmt.GetValueOrDefault() - import
      .Ocse34.SuppDisbLegal.GetValueOrDefault() - import
      .Ocse34.SuspCrdForFut.GetValueOrDefault();

    import.Group.Index = 60;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Perform line 9b Column G out of balance check.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 36;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 37;
    import.Group.CheckSize();

    local.Amount.TotalCurrency -= import.Group.Item.Common.TotalCurrency;
    local.AmountCompare.TotalCurrency = 0;

    import.Group.Index = 59;
    import.Group.CheckSize();

    local.AmountCompare.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 60;
    import.Group.CheckSize();

    local.AmountCompare.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    if (local.AmountCompare.TotalCurrency == local.Amount.TotalCurrency)
    {
      export.Line9BOobInd.Flag = "N";
    }
    else
    {
      export.Line9BOobInd.Flag = "Y";
    }

    // ***************************************************************************
    // **       Calculate line 10A Column B.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 18;
    import.Group.CheckSize();

    local.Amount.TotalCurrency = import.Group.Item.Common.TotalCurrency * import
      .Ocse34.FmapRate;

    import.Group.Index = 40;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 10A Column D.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 52;
    import.Group.CheckSize();

    local.Amount.TotalCurrency = import.Group.Item.Common.TotalCurrency * import
      .Ocse34.FmapRate;

    import.Group.Index = 61;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 10A Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 40;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 61;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 42;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 10B Column A.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 17;
    import.Group.CheckSize();

    local.Amount.TotalCurrency = import.Group.Item.Common.TotalCurrency * import
      .Ocse34.FmapRate;

    import.Group.Index = 39;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 10B Column C.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 19;
    import.Group.CheckSize();

    local.Amount.TotalCurrency = import.Group.Item.Common.TotalCurrency * import
      .Ocse34.FmapRate;

    import.Group.Index = 41;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 10B Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 39;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 41;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 62;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Calculate line 11 Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 49;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // --  Line 12 column G is no longer on the report.  This line was reporting
    // the Quarterly Grant Adjustment amount.  I'm leaving the calculation
    // intact at this point in case they want it added back later.  It shouldn't
    // hurt anything to do the calculation, it simply won't display anywhere on
    // the report.
    // ***************************************************************************
    // **       Calculate line 12 Column G.
    // ***************************************************************************
    local.Amount.TotalCurrency = 0;

    import.Group.Index = 62;
    import.Group.CheckSize();

    local.Amount.TotalCurrency += import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 45;
    import.Group.CheckSize();

    local.Amount.TotalCurrency -= import.Group.Item.Common.TotalCurrency;

    import.Group.Index = 48;
    import.Group.CheckSize();

    import.Group.Update.Common.TotalCurrency = local.Amount.TotalCurrency;

    // ***************************************************************************
    // **       Apply totals to database.
    // ***************************************************************************
    UseFnB700ApplyUpdates();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      local.ForError.LineNumber = "06";
      UseOcse157WriteError();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Abort.Flag = "Y";
      }
    }
  }

  private static void MoveGroup1(Import.GroupGroup source,
    FnB700ApplyUpdates.Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveGroup2(FnB700BuildGvForRestart.Export.
    GroupGroup source, Import.GroupGroup target)
  {
    target.Common.TotalCurrency = source.Common.TotalCurrency;
  }

  private static void MoveOcse1(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
  }

  private static void MoveOcse2(Ocse34 source, Ocse34 target)
  {
    target.Period = source.Period;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.FmapRate = source.FmapRate;
  }

  private void UseFnB700ApplyUpdates()
  {
    var useImport = new FnB700ApplyUpdates.Import();
    var useExport = new FnB700ApplyUpdates.Export();

    MoveOcse2(import.Ocse34, useImport.Ocse34);
    import.Group.CopyTo(useImport.Group, MoveGroup1);
    useImport.Lda.TotalCurrency = local.Lda.TotalCurrency;

    Call(FnB700ApplyUpdates.Execute, useImport, useExport);
  }

  private void UseFnB700BuildGvForRestart()
  {
    var useImport = new FnB700BuildGvForRestart.Import();
    var useExport = new FnB700BuildGvForRestart.Export();

    MoveOcse1(import.Ocse34, useImport.Ocse34);

    Call(FnB700BuildGvForRestart.Execute, useImport, useExport);

    useExport.Group.CopyTo(import.Group, MoveGroup2);
  }

  private void UseOcse157WriteError()
  {
    var useImport = new Ocse157WriteError.Import();
    var useExport = new Ocse157WriteError.Export();

    useImport.Ocse157Verification.LineNumber = local.ForError.LineNumber;

    Call(Ocse157WriteError.Execute, useImport, useExport);
  }

  private bool ReadOcse34()
  {
    entities.Ocse34.Populated = false;

    return Read("ReadOcse34",
      (db, command) =>
      {
        db.SetInt32(command, "period", import.Prev.Period);
      },
      (db, reader) =>
      {
        entities.Ocse34.Period = db.GetInt32(reader, 0);
        entities.Ocse34.PreviousUndistribAmount = db.GetInt32(reader, 1);
        entities.Ocse34.TotalCollectionsAmount = db.GetInt32(reader, 2);
        entities.Ocse34.OtherStateAmtForward = db.GetNullableInt32(reader, 3);
        entities.Ocse34.AvailForDistributionAmount =
          db.GetNullableInt32(reader, 4);
        entities.Ocse34.DistribAssistReimbAmount =
          db.GetNullableInt32(reader, 5);
        entities.Ocse34.DistributedMedSupportAmount =
          db.GetNullableInt32(reader, 6);
        entities.Ocse34.DistributedFamilyAmount =
          db.GetNullableInt32(reader, 7);
        entities.Ocse34.TotalDistributedIvaAmount =
          db.GetNullableInt32(reader, 8);
        entities.Ocse34.TotalDistributedIveAmount =
          db.GetNullableInt32(reader, 9);
        entities.Ocse34.TotalDistributedFormerAmount =
          db.GetNullableInt32(reader, 10);
        entities.Ocse34.TotalDistributedNeverAmount =
          db.GetNullableInt32(reader, 11);
        entities.Ocse34.TotalDistributedAmount =
          db.GetNullableInt32(reader, 12);
        entities.Ocse34.GrossUndistributedAmount =
          db.GetNullableInt32(reader, 13);
        entities.Ocse34.NetUndistributedAmount =
          db.GetNullableInt32(reader, 14);
        entities.Ocse34.FederalShareIvaAmount = db.GetNullableInt32(reader, 15);
        entities.Ocse34.FederalShareIveAmount = db.GetNullableInt32(reader, 16);
        entities.Ocse34.FederalShareFormerAmount =
          db.GetNullableInt32(reader, 17);
        entities.Ocse34.FederalShareTotalAmount =
          db.GetNullableInt32(reader, 18);
        entities.Ocse34.CreatedTimestamp = db.GetDateTime(reader, 19);
        entities.Ocse34.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 74;

      private Common common;
    }

    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public Ocse34 Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of ProgramCheckpointRestart.
    /// </summary>
    [JsonPropertyName("programCheckpointRestart")]
    public ProgramCheckpointRestart ProgramCheckpointRestart
    {
      get => programCheckpointRestart ??= new();
      set => programCheckpointRestart = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private Ocse34 ocse34;
    private Ocse34 prev;
    private ProgramCheckpointRestart programCheckpointRestart;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Line9BOobInd.
    /// </summary>
    [JsonPropertyName("line9BOobInd")]
    public Common Line9BOobInd
    {
      get => line9BOobInd ??= new();
      set => line9BOobInd = value;
    }

    /// <summary>
    /// A value of Line9OobInd.
    /// </summary>
    [JsonPropertyName("line9OobInd")]
    public Common Line9OobInd
    {
      get => line9OobInd ??= new();
      set => line9OobInd = value;
    }

    /// <summary>
    /// A value of Abort.
    /// </summary>
    [JsonPropertyName("abort")]
    public Common Abort
    {
      get => abort ??= new();
      set => abort = value;
    }

    private Common line9BOobInd;
    private Common line9OobInd;
    private Common abort;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Lda.
    /// </summary>
    [JsonPropertyName("lda")]
    public Common Lda
    {
      get => lda ??= new();
      set => lda = value;
    }

    /// <summary>
    /// A value of AmountCompare.
    /// </summary>
    [JsonPropertyName("amountCompare")]
    public Common AmountCompare
    {
      get => amountCompare ??= new();
      set => amountCompare = value;
    }

    /// <summary>
    /// A value of Amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public Common Amount
    {
      get => amount ??= new();
      set => amount = value;
    }

    /// <summary>
    /// A value of ForError.
    /// </summary>
    [JsonPropertyName("forError")]
    public Ocse157Verification ForError
    {
      get => forError ??= new();
      set => forError = value;
    }

    private Common lda;
    private Common amountCompare;
    private Common amount;
    private Ocse157Verification forError;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ocse34.
    /// </summary>
    [JsonPropertyName("ocse34")]
    public Ocse34 Ocse34
    {
      get => ocse34 ??= new();
      set => ocse34 = value;
    }

    private Ocse34 ocse34;
  }
#endregion
}
