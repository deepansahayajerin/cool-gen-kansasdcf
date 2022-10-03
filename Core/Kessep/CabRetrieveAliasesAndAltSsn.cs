// Program: CAB_RETRIEVE_ALIASES_AND_ALT_SSN, ID: 371051271, model: 746.
// Short name: SWE01282
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_RETRIEVE_ALIASES_AND_ALT_SSN.
/// </summary>
[Serializable]
public partial class CabRetrieveAliasesAndAltSsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_RETRIEVE_ALIASES_AND_ALT_SSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabRetrieveAliasesAndAltSsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabRetrieveAliasesAndAltSsn.
  /// </summary>
  public CabRetrieveAliasesAndAltSsn(IContext context, Import import,
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
    local.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    if (AsChar(import.Batch.Flag) == 'Y')
    {
      UseEabReadAliasBatch();
    }
    else
    {
      UseEabReadAlias();
    }

    // ************************************************
    // *Interpret the error codes returned from ADABAS*
    // *and set an appropriate exit state.            *
    // ************************************************
    switch(AsChar(local.AbendData.Type1))
    {
      case ' ':
        // ************************************************
        // *Successful Adabas Read Occurred.             *
        // ************************************************
        break;
      case 'A':
        // ************************************************
        // *Unsuccessful ADABAS Read Occurred.           *
        // ************************************************
        switch(TrimEnd(local.AbendData.AdabasResponseCd))
        {
          case "0113":
            ExitState = "ACO_ADABAS_PERSON_NF_113";

            break;
          case "0148":
            ExitState = "ACO_ADABAS_UNAVAILABLE";

            break;
          default:
            ExitState = "ADABAS_READ_UNSUCCESSFUL";

            break;
        }

        return;
      case 'C':
        // ************************************************
        // *CICS action Failed. A reason code should be   *
        // *interpreted.
        // 
        // *
        // ************************************************
        if (IsEmpty(local.AbendData.CicsResponseCd))
        {
        }
        else
        {
          ExitState = "ACO_NE0000_CICS_UNAVAILABLE";

          return;
        }

        break;
      default:
        ExitState = "ADABAS_INVALID_RETURN_CODE";

        return;
    }

    // **********************************************************************
    // If any alias names are retrieved, format the input name before doing
    // any comparisons.  Otherwise, get out.
    // **********************************************************************
    if (local.Aliases.IsEmpty)
    {
      return;
    }

    UseCabFcrFormatNames1();

    // **********************************************************************
    // Retrieve any alias names that do not duplicate the import name,
    // then remove special characters including spaces.
    // **********************************************************************
    for(local.Aliases.Index = 0; local.Aliases.Index < local.Aliases.Count; ++
      local.Aliases.Index)
    {
      if (!IsEmpty(local.Aliases.Item.G.LastName) && !
        IsEmpty(local.Aliases.Item.G.FirstName))
      {
        local.FormattedAlias.Assign(local.Aliases.Item.G);
        UseCabFcrFormatNames2();

        if (Equal(local.FormattedAlias.FirstName,
          local.FormattedImport.FirstName) && AsChar
          (local.FormattedAlias.MiddleInitial) == AsChar
          (local.FormattedImport.MiddleInitial) && Equal
          (local.FormattedAlias.LastName, local.FormattedImport.LastName))
        {
        }
        else
        {
          for(export.Names.Index = 0; export.Names.Index < export.Names.Count; ++
            export.Names.Index)
          {
            if (!export.Names.CheckSize())
            {
              break;
            }

            if (Equal(local.FormattedAlias.FirstName,
              export.Names.Item.Gnames.FirstName) && AsChar
              (local.FormattedAlias.MiddleInitial) == AsChar
              (export.Names.Item.Gnames.MiddleInitial) && Equal
              (local.FormattedAlias.LastName, export.Names.Item.Gnames.LastName))
              
            {
              goto Next1;
            }
          }

          export.Names.CheckIndex();

          export.Names.Index = export.Names.Count;
          export.Names.CheckSize();

          export.Names.Update.Gnames.Assign(local.FormattedAlias);
        }
      }

Next1:
      ;
    }

    // **********************************************************************
    // Retrieve any alias names that do not duplicate the import name,
    // then remove special characters including spaces.
    // **********************************************************************
    for(local.Aliases.Index = 0; local.Aliases.Index < local.Aliases.Count; ++
      local.Aliases.Index)
    {
      if (Lt("000000000", local.Aliases.Item.G.Ssn) && Lt
        (local.Aliases.Item.G.Ssn, "999999999") && !
        Equal(local.Aliases.Item.G.Ssn, import.CsePersonsWorkSet.Ssn))
      {
        for(export.AlternateSsn.Index = 0; export.AlternateSsn.Index < export
          .AlternateSsn.Count; ++export.AlternateSsn.Index)
        {
          if (!export.AlternateSsn.CheckSize())
          {
            break;
          }

          if (Equal(local.Aliases.Item.G.Ssn, export.AlternateSsn.Item.Gssn.Ssn))
            
          {
            goto Next2;
          }
        }

        export.AlternateSsn.CheckIndex();

        export.AlternateSsn.Index = export.AlternateSsn.Count;
        export.AlternateSsn.CheckSize();

        export.AlternateSsn.Update.Gssn.Ssn = local.Aliases.Item.G.Ssn;
      }

Next2:
      ;
    }
  }

  private static void MoveAliases1(Local.AliasesGroup source,
    EabReadAlias.Export.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.GcseN.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveAliases2(Local.AliasesGroup source,
    EabReadAliasBatch.Export.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.GcseN.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveAliases3(EabReadAlias.Export.AliasesGroup source,
    Local.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.GcseN.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveAliases4(EabReadAliasBatch.Export.AliasesGroup source,
    Local.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.GcseN.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private void UseCabFcrFormatNames1()
  {
    var useImport = new CabFcrFormatNames.Import();
    var useExport = new CabFcrFormatNames.Export();

    useImport.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    Call(CabFcrFormatNames.Execute, useImport, useExport);

    local.FormattedImport.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabFcrFormatNames2()
  {
    var useImport = new CabFcrFormatNames.Import();
    var useExport = new CabFcrFormatNames.Export();

    useImport.CsePersonsWorkSet.Assign(local.FormattedAlias);

    Call(CabFcrFormatNames.Execute, useImport, useExport);

    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, local.FormattedAlias);
  }

  private void UseEabReadAlias()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    MoveCsePersonsWorkSet1(local.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useExport.AbendData.Assign(local.AbendData);
    local.Aliases.CopyTo(useExport.Aliases, MoveAliases1);

    Call(EabReadAlias.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.Aliases.CopyTo(local.Aliases, MoveAliases3);
  }

  private void UseEabReadAliasBatch()
  {
    var useImport = new EabReadAliasBatch.Import();
    var useExport = new EabReadAliasBatch.Export();

    MoveCsePersonsWorkSet1(local.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      
    useExport.AbendData.Assign(local.AbendData);
    local.Aliases.CopyTo(useExport.Aliases, MoveAliases2);

    Call(EabReadAliasBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    useExport.Aliases.CopyTo(local.Aliases, MoveAliases4);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common batch;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A NamesGroup group.</summary>
    [Serializable]
    public class NamesGroup
    {
      /// <summary>
      /// A value of Gnames.
      /// </summary>
      [JsonPropertyName("gnames")]
      public CsePersonsWorkSet Gnames
      {
        get => gnames ??= new();
        set => gnames = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gnames;
    }

    /// <summary>A AlternateSsnGroup group.</summary>
    [Serializable]
    public class AlternateSsnGroup
    {
      /// <summary>
      /// A value of Gssn.
      /// </summary>
      [JsonPropertyName("gssn")]
      public CsePersonsWorkSet Gssn
      {
        get => gssn ??= new();
        set => gssn = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet gssn;
    }

    /// <summary>
    /// Gets a value of Names.
    /// </summary>
    [JsonIgnore]
    public Array<NamesGroup> Names => names ??= new(NamesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Names for json serialization.
    /// </summary>
    [JsonPropertyName("names")]
    [Computed]
    public IList<NamesGroup> Names_Json
    {
      get => names;
      set => Names.Assign(value);
    }

    /// <summary>
    /// Gets a value of AlternateSsn.
    /// </summary>
    [JsonIgnore]
    public Array<AlternateSsnGroup> AlternateSsn => alternateSsn ??= new(
      AlternateSsnGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of AlternateSsn for json serialization.
    /// </summary>
    [JsonPropertyName("alternateSsn")]
    [Computed]
    public IList<AlternateSsnGroup> AlternateSsn_Json
    {
      get => alternateSsn;
      set => AlternateSsn.Assign(value);
    }

    private Array<NamesGroup> names;
    private Array<AlternateSsnGroup> alternateSsn;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A AliasesGroup group.</summary>
    [Serializable]
    public class AliasesGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public CsePersonsWorkSet G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>
      /// A value of Gkscares.
      /// </summary>
      [JsonPropertyName("gkscares")]
      public Common Gkscares
      {
        get => gkscares ??= new();
        set => gkscares = value;
      }

      /// <summary>
      /// A value of Gkanpay.
      /// </summary>
      [JsonPropertyName("gkanpay")]
      public Common Gkanpay
      {
        get => gkanpay ??= new();
        set => gkanpay = value;
      }

      /// <summary>
      /// A value of GcseN.
      /// </summary>
      [JsonPropertyName("gcseN")]
      public Common GcseN
      {
        get => gcseN ??= new();
        set => gcseN = value;
      }

      /// <summary>
      /// A value of Gae.
      /// </summary>
      [JsonPropertyName("gae")]
      public Common Gae
      {
        get => gae ??= new();
        set => gae = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private CsePersonsWorkSet g;
      private Common gkscares;
      private Common gkanpay;
      private Common gcseN;
      private Common gae;
    }

    /// <summary>
    /// A value of FormattedImport.
    /// </summary>
    [JsonPropertyName("formattedImport")]
    public CsePersonsWorkSet FormattedImport
    {
      get => formattedImport ??= new();
      set => formattedImport = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// Gets a value of Aliases.
    /// </summary>
    [JsonIgnore]
    public Array<AliasesGroup> Aliases =>
      aliases ??= new(AliasesGroup.Capacity);

    /// <summary>
    /// Gets a value of Aliases for json serialization.
    /// </summary>
    [JsonPropertyName("aliases")]
    [Computed]
    public IList<AliasesGroup> Aliases_Json
    {
      get => aliases;
      set => Aliases.Assign(value);
    }

    /// <summary>
    /// A value of FormattedAlias.
    /// </summary>
    [JsonPropertyName("formattedAlias")]
    public CsePersonsWorkSet FormattedAlias
    {
      get => formattedAlias ??= new();
      set => formattedAlias = value;
    }

    private CsePersonsWorkSet formattedImport;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private Array<AliasesGroup> aliases;
    private CsePersonsWorkSet formattedAlias;
  }
#endregion
}
