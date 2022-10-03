// Program: OE_CAB_READ_INFRASTRUCTURE, ID: 371731792, model: 746.
// Short name: SWE01897
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CAB_READ_INFRASTRUCTURE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Written by Raju     : Dec 11 1996
/// This action block raises the event(s) for Infrastructure for following prads
/// 	BKRP, MILI, JAIL, INCH, INCS, APDS
/// </para>
/// </summary>
[Serializable]
public partial class OeCabReadInfrastructure: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_READ_INFRASTRUCTURE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabReadInfrastructure(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabReadInfrastructure.
  /// </summary>
  public OeCabReadInfrastructure(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 01/20/97	raju	initial creation
    // 			1045hrs CST
    // --------------------------------------------
    export.Infrastructure.Assign(local.Blank);

    if (ReadInfrastructure())
    {
      export.Infrastructure.Assign(entities.Infrastructure);
    }
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
        entities.Infrastructure.DenormNumeric12 =
          db.GetNullableInt64(reader, 1);
        entities.Infrastructure.DenormText12 = db.GetNullableString(reader, 2);
        entities.Infrastructure.DenormDate = db.GetNullableDate(reader, 3);
        entities.Infrastructure.DenormTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.Infrastructure.CaseNumber = db.GetNullableString(reader, 5);
        entities.Infrastructure.CsePersonNumber =
          db.GetNullableString(reader, 6);
        entities.Infrastructure.CaseUnitNumber = db.GetNullableInt32(reader, 7);
        entities.Infrastructure.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public Infrastructure Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private Infrastructure blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
