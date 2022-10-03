// Program: SI_GEN_INCOME_SRCE_CONTACT_ID, ID: 371763806, model: 746.
// Short name: SWE01169
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_GEN_INCOME_SRCE_CONTACT_ID.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block generates the next available number of an income source 
/// contact for a given income source of a given CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class SiGenIncomeSrceContactId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GEN_INCOME_SRCE_CONTACT_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGenIncomeSrceContactId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGenIncomeSrceContactId.
  /// </summary>
  public SiGenIncomeSrceContactId(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.ControlTable.Identifier = "INCOME SOURCE CONTACT";
    UseAccessControlTable();
    export.IncomeSourceContact.Identifier = local.ControlTable.LastUsedNumber;
  }

  private void UseAccessControlTable()
  {
    var useImport = new AccessControlTable.Import();
    var useExport = new AccessControlTable.Export();

    useImport.ControlTable.Identifier = local.ControlTable.Identifier;

    Call(AccessControlTable.Execute, useImport, useExport);

    local.ControlTable.LastUsedNumber = useExport.ControlTable.LastUsedNumber;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// <summary>
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
    }

    private IncomeSourceContact incomeSourceContact;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private ControlTable controlTable;
  }
#endregion
}
