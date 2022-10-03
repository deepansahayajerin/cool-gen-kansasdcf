// Program: SI_QUICK_AUDIT_MAIN, ID: 374537051, model: 746.
// Short name: SWEQKAUP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_AUDIT_MAIN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class SiQuickAuditMain: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_AUDIT_MAIN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickAuditMain(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickAuditMain.
  /// </summary>
  public SiQuickAuditMain(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    // This procedure step functions as a non-display server
    // and is the target of a QUICK application COM proxy.
    // Any changes to the import/export views of this procedure
    // step MUST be coordinated, as such changes will impact the
    // calling COM proxy.
    // ------------------------------------------------------------
    // ----------------------------------------------------------------------------
    // Date		Developer	Request #	Description
    // ----------------------------------------------------------------------------
    // 01/2010    	T. Pierce	# 211		Initial development
    // ----------------------------------------------------------------------------
    UseSiQuickAudit();
  }

  private void UseSiQuickAudit()
  {
    var useImport = new SiQuickAudit.Import();
    var useExport = new SiQuickAudit.Export();

    useImport.QuickAudit.Assign(import.QuickAudit);

    Call(SiQuickAudit.Execute, useImport, useExport);

    export.QuickAudit.Assign(useExport.QuickAudit);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of QuickAudit.
    /// </summary>
    [JsonPropertyName("quickAudit")]
    public QuickAudit QuickAudit
    {
      get => quickAudit ??= new();
      set => quickAudit = value;
    }

    private QuickAudit quickAudit;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of QuickAudit.
    /// </summary>
    [JsonPropertyName("quickAudit")]
    public QuickAudit QuickAudit
    {
      get => quickAudit ??= new();
      set => quickAudit = value;
    }

    private QuickAudit quickAudit;
  }
#endregion
}
