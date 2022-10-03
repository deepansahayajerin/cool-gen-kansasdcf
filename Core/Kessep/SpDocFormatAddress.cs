// Program: SP_DOC_FORMAT_ADDRESS, ID: 372134096, model: 746.
// Short name: SWE01653
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
/// A program: SP_DOC_FORMAT_ADDRESS.
/// </para>
/// <para>
/// Formats address for documents
/// </para>
/// </summary>
[Serializable]
public partial class SpDocFormatAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DOC_FORMAT_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDocFormatAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDocFormatAddress.
  /// </summary>
  public SpDocFormatAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // mjr
    // --------------------------------------------------------
    // 10/12/1998	M Ramirez	Added code to place output in
    // 				group view
    // 03/25/1999	M Ramirez	Added check for blanks in
    // 				Street3 and Street4 in Foreign
    // 				addresses
    // -----------------------------------------------------------
    // -----------------------------------------------------------------
    // Address is comprised of : Street1, Street2, City, State
    // Zip-Zip4-Zip3
    // CSE Person address may be foreign too. In which case it
    // is Street1, Street2, Street3, Street4, City, Province,
    // Country, Postal code.
    // -----------------------------------------------------------------
    // -----------------------------------------------------------------
    // Street 1
    // -----------------------------------------------------------------
    export.Export1.Index = 0;
    export.Export1.CheckSize();

    export.Export1.Update.G.Value = import.SpPrintWorkSet.Street1;

    // -----------------------------------------------------------------
    // Street 2
    // -----------------------------------------------------------------
    ++export.Export1.Index;
    export.Export1.CheckSize();

    export.Export1.Update.G.Value = import.SpPrintWorkSet.Street2;

    if (AsChar(import.SpPrintWorkSet.LocationType) == 'F')
    {
      if (!IsEmpty(import.SpPrintWorkSet.Street3))
      {
        // -----------------------------------------------------------------
        // Street 3
        // -----------------------------------------------------------------
        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.G.Value = import.SpPrintWorkSet.Street3;
      }

      if (!IsEmpty(import.SpPrintWorkSet.Street4))
      {
        // -----------------------------------------------------------------
        // Street 4
        // -----------------------------------------------------------------
        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.G.Value = import.SpPrintWorkSet.Street4;
      }

      // -----------------------------------------------------------------
      // City
      // -----------------------------------------------------------------
      ++export.Export1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.G.Value = import.SpPrintWorkSet.City;

      // -----------------------------------------------------------------
      // Province
      // -----------------------------------------------------------------
      if (!IsEmpty(import.SpPrintWorkSet.Province))
      {
        export.Export1.Update.G.Value = TrimEnd(export.Export1.Item.G.Value) + ", " +
          import.SpPrintWorkSet.Province;
      }

      // -----------------------------------------------------------------
      // Country
      // -----------------------------------------------------------------
      local.Code.CodeName = "COUNTRY CODE";
      local.CodeValue.Cdvalue = import.SpPrintWorkSet.Country;
      UseCabGetCodeValueDescription();
      export.Export1.Update.G.Value = TrimEnd(export.Export1.Item.G.Value) + ", " +
        local.CodeValue.Description;

      // -----------------------------------------------------------------
      // Postal Code
      // -----------------------------------------------------------------
      export.Export1.Update.G.Value = TrimEnd(export.Export1.Item.G.Value) + "  " +
        import.SpPrintWorkSet.PostalCode;
    }
    else
    {
      // -----------------------------------------------------------------
      // City, State  Zip5
      // -----------------------------------------------------------------
      ++export.Export1.Index;
      export.Export1.CheckSize();

      export.Export1.Update.G.Value = TrimEnd(import.SpPrintWorkSet.City) + ", " +
        TrimEnd(import.SpPrintWorkSet.State) + "  " + import
        .SpPrintWorkSet.ZipCode;

      // -----------------------------------------------------------------
      // Zip4
      // -----------------------------------------------------------------
      if (!IsEmpty(import.SpPrintWorkSet.Zip4))
      {
        export.Export1.Update.G.Value = TrimEnd(export.Export1.Item.G.Value) + "-"
          + import.SpPrintWorkSet.Zip4;

        // -----------------------------------------------------------------
        // Zip3
        // -----------------------------------------------------------------
        if (!IsEmpty(import.SpPrintWorkSet.Zip3))
        {
          export.Export1.Update.G.Value =
            TrimEnd(export.Export1.Item.G.Value) + "-" + import
            .SpPrintWorkSet.Zip3;
        }
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private void UseCabGetCodeValueDescription()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
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
    /// <summary>
    /// A value of SpPrintWorkSet.
    /// </summary>
    [JsonPropertyName("spPrintWorkSet")]
    public SpPrintWorkSet SpPrintWorkSet
    {
      get => spPrintWorkSet ??= new();
      set => spPrintWorkSet = value;
    }

    private SpPrintWorkSet spPrintWorkSet;
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
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public FieldValue G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private FieldValue g;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public FieldValue Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    private Array<ExportGroup> export1;
    private FieldValue zdel;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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

    private Code code;
    private CodeValue codeValue;
  }
#endregion
}
