// Program: SI_ALTS_BUILD_ALIAS_AND_SSN, ID: 371755383, model: 746.
// Short name: SWE01101
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ALTS_BUILD_ALIAS_AND_SSN.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiAltsBuildAliasAndSsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ALTS_BUILD_ALIAS_AND_SSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAltsBuildAliasAndSsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAltsBuildAliasAndSsn.
  /// </summary>
  public SiAltsBuildAliasAndSsn(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //       M A I N T E N A N C E   L O G
    //   Date    Developer	Description
    // 09-06-95  K Evans  	Initial development
    // 06/27/96  G. Lofton	Added check if group view is empty
    // ---------------------------------------------
    // 	
    // 02/11/99 W.Campbell     Fixed the target on
    //                         two move statements which
    //                         were giving consistency check
    //                         warnings because they would
    //                         not move anything. i.e. there
    //                         were no attributes in common
    //                         in the source and target views.
    // ---------------------------------------------
    // 	
    // *********************************************
    // Read for the AP
    // *********************************************
    if (!IsEmpty(import.Ap1.Number))
    {
      UseEabReadAlias1();

      switch(AsChar(local.AbendData.Type1))
      {
        case ' ':
          // *********************************************
          // Successful read
          // *********************************************
          if (!local.Group.IsEmpty)
          {
            for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
              local.Group.Index)
            {
              if (!local.Group.CheckSize())
              {
                break;
              }

              export.Ap.Index = local.Group.Index;
              export.Ap.CheckSize();

              export.Ap.Update.GapCsePersonsWorkSet.Assign(local.Group.Item.G);

              // ---------------------------------------------
              // 02/11/99 W.Campbell - Fixed the target on
              // the following move statement which was giving
              // consistency check warnings because it
              // would not move anything. i.e. there were no
              // attributes in common in the source and
              // target views.
              // ---------------------------------------------
              // 	
              export.Ap.Update.GapAe.Flag = local.Group.Item.Gae.Flag;
              export.Ap.Update.GapCse.Flag = local.Group.Item.Gcse.Flag;
              export.Ap.Update.GapKanpay.Flag = local.Group.Item.Gkanpay.Flag;
              export.Ap.Update.GapKscares.Flag = local.Group.Item.Gkscares.Flag;
              export.Ap.Update.GapSsn3.Text3 =
                Substring(local.Group.Item.G.Ssn, 1, 3);
              export.Ap.Update.GapSsn2.Text3 =
                Substring(local.Group.Item.G.Ssn, 4, 2);
              export.Ap.Update.GapSsn4.Text4 =
                Substring(local.Group.Item.G.Ssn, 6, 4);
              export.Ap.Update.GapDbOccurrence.Flag = "Y";
            }

            local.Group.CheckIndex();
            export.ApOccur.Flag = "Y";
          }

          break;
        case 'A':
          // *********************************************
          // ADABAS read failed.
          // *********************************************
          if (IsEmpty(local.AbendData.AdabasResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_ADABAS_UNAVAILABLE";
          }

          break;
        case 'C':
          // *********************************************
          // CICS action failed.
          // *********************************************
          if (IsEmpty(local.AbendData.CicsResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
          }

          break;
        default:
          break;
      }
    }

    local.Group.Count = 0;

    // *********************************************
    // Read for the AR
    // *********************************************
    if (!IsEmpty(import.Ar1.Number))
    {
      if (CharAt(import.Ar1.Number, 10) == 'O')
      {
        return;
      }

      UseEabReadAlias2();

      switch(AsChar(local.AbendData.Type1))
      {
        case ' ':
          // *********************************************
          // Successful read
          // *********************************************
          if (!local.Group.IsEmpty)
          {
            for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
              local.Group.Index)
            {
              if (!local.Group.CheckSize())
              {
                break;
              }

              export.Ar.Index = local.Group.Index;
              export.Ar.CheckSize();

              export.Ar.Update.GarCsePersonsWorkSet.Assign(local.Group.Item.G);

              // ---------------------------------------------
              // 02/11/99 W.Campbell - Fixed the target on
              // the following move statement which was giving
              // consistency check warnings because it
              // would not move anything. i.e. there were no
              // attributes in common in the source and
              // target views.
              // ---------------------------------------------
              // 	
              export.Ar.Update.GarAe.Flag = local.Group.Item.Gae.Flag;
              export.Ar.Update.GarCse.Flag = local.Group.Item.Gcse.Flag;
              export.Ar.Update.GarKanpay.Flag = local.Group.Item.Gkanpay.Flag;
              export.Ar.Update.GarKscares.Flag = local.Group.Item.Gkscares.Flag;
              export.Ar.Update.GarSsn3.Text3 =
                Substring(local.Group.Item.G.Ssn, 1, 3);
              export.Ar.Update.GarSsn2.Text3 =
                Substring(local.Group.Item.G.Ssn, 4, 2);
              export.Ar.Update.GarSsn4.Text4 =
                Substring(local.Group.Item.G.Ssn, 6, 4);
              export.Ar.Update.GarDbOccurrence.Flag = "Y";
            }

            local.Group.CheckIndex();
            export.ArOccur.Flag = "Y";
          }

          break;
        case 'A':
          // *********************************************
          // ADABAS read failed.
          // *********************************************
          if (IsEmpty(local.AbendData.AdabasResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_ADABAS_UNAVAILABLE";
          }

          break;
        case 'C':
          // *********************************************
          // CICS action failed.
          // *********************************************
          if (IsEmpty(local.AbendData.CicsResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
          }

          break;
        default:
          break;
      }
    }
  }

  private static void MoveAliasesToGroup(EabReadAlias.Export.
    AliasesGroup source, Local.GroupGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private static void MoveGroupToAliases(Local.GroupGroup source,
    EabReadAlias.Export.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
  }

  private void UseEabReadAlias1()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    MoveCsePersonsWorkSet(import.Ap1, useImport.CsePersonsWorkSet);
    local.Group.CopyTo(useExport.Aliases, MoveGroupToAliases);
    useExport.AbendData.Assign(local.AbendData);
    useExport.NextKey.UniqueKey = export.NextKeyAp.UniqueKey;

    Call(EabReadAlias.Execute, useImport, useExport);

    useExport.Aliases.CopyTo(local.Group, MoveAliasesToGroup);
    local.AbendData.Assign(useExport.AbendData);
    export.NextKeyAp.UniqueKey = useExport.NextKey.UniqueKey;
  }

  private void UseEabReadAlias2()
  {
    var useImport = new EabReadAlias.Import();
    var useExport = new EabReadAlias.Export();

    MoveCsePersonsWorkSet(import.Ar1, useImport.CsePersonsWorkSet);
    local.Group.CopyTo(useExport.Aliases, MoveGroupToAliases);
    useExport.AbendData.Assign(local.AbendData);
    useExport.NextKey.UniqueKey = export.NextKeyAr.UniqueKey;

    Call(EabReadAlias.Execute, useImport, useExport);

    useExport.Aliases.CopyTo(local.Group, MoveAliasesToGroup);
    local.AbendData.Assign(useExport.AbendData);
    export.NextKeyAr.UniqueKey = useExport.NextKey.UniqueKey;
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
    /// A value of Ap1.
    /// </summary>
    [JsonPropertyName("ap1")]
    public CsePersonsWorkSet Ap1
    {
      get => ap1 ??= new();
      set => ap1 = value;
    }

    /// <summary>
    /// A value of Ar1.
    /// </summary>
    [JsonPropertyName("ar1")]
    public CsePersonsWorkSet Ar1
    {
      get => ar1 ??= new();
      set => ar1 = value;
    }

    private CsePersonsWorkSet ap1;
    private CsePersonsWorkSet ar1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of GapCommon.
      /// </summary>
      [JsonPropertyName("gapCommon")]
      public Common GapCommon
      {
        get => gapCommon ??= new();
        set => gapCommon = value;
      }

      /// <summary>
      /// A value of GapCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gapCsePersonsWorkSet")]
      public CsePersonsWorkSet GapCsePersonsWorkSet
      {
        get => gapCsePersonsWorkSet ??= new();
        set => gapCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GapSsn3.
      /// </summary>
      [JsonPropertyName("gapSsn3")]
      public WorkArea GapSsn3
      {
        get => gapSsn3 ??= new();
        set => gapSsn3 = value;
      }

      /// <summary>
      /// A value of GapSsn2.
      /// </summary>
      [JsonPropertyName("gapSsn2")]
      public WorkArea GapSsn2
      {
        get => gapSsn2 ??= new();
        set => gapSsn2 = value;
      }

      /// <summary>
      /// A value of GapSsn4.
      /// </summary>
      [JsonPropertyName("gapSsn4")]
      public WorkArea GapSsn4
      {
        get => gapSsn4 ??= new();
        set => gapSsn4 = value;
      }

      /// <summary>
      /// A value of GapKscares.
      /// </summary>
      [JsonPropertyName("gapKscares")]
      public Common GapKscares
      {
        get => gapKscares ??= new();
        set => gapKscares = value;
      }

      /// <summary>
      /// A value of GapKanpay.
      /// </summary>
      [JsonPropertyName("gapKanpay")]
      public Common GapKanpay
      {
        get => gapKanpay ??= new();
        set => gapKanpay = value;
      }

      /// <summary>
      /// A value of GapCse.
      /// </summary>
      [JsonPropertyName("gapCse")]
      public Common GapCse
      {
        get => gapCse ??= new();
        set => gapCse = value;
      }

      /// <summary>
      /// A value of GapAe.
      /// </summary>
      [JsonPropertyName("gapAe")]
      public Common GapAe
      {
        get => gapAe ??= new();
        set => gapAe = value;
      }

      /// <summary>
      /// A value of GapDbOccurrence.
      /// </summary>
      [JsonPropertyName("gapDbOccurrence")]
      public Common GapDbOccurrence
      {
        get => gapDbOccurrence ??= new();
        set => gapDbOccurrence = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common gapCommon;
      private CsePersonsWorkSet gapCsePersonsWorkSet;
      private WorkArea gapSsn3;
      private WorkArea gapSsn2;
      private WorkArea gapSsn4;
      private Common gapKscares;
      private Common gapKanpay;
      private Common gapCse;
      private Common gapAe;
      private Common gapDbOccurrence;
    }

    /// <summary>A ArGroup group.</summary>
    [Serializable]
    public class ArGroup
    {
      /// <summary>
      /// A value of GarCommon.
      /// </summary>
      [JsonPropertyName("garCommon")]
      public Common GarCommon
      {
        get => garCommon ??= new();
        set => garCommon = value;
      }

      /// <summary>
      /// A value of GarCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("garCsePersonsWorkSet")]
      public CsePersonsWorkSet GarCsePersonsWorkSet
      {
        get => garCsePersonsWorkSet ??= new();
        set => garCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GarSsn3.
      /// </summary>
      [JsonPropertyName("garSsn3")]
      public WorkArea GarSsn3
      {
        get => garSsn3 ??= new();
        set => garSsn3 = value;
      }

      /// <summary>
      /// A value of GarSsn2.
      /// </summary>
      [JsonPropertyName("garSsn2")]
      public WorkArea GarSsn2
      {
        get => garSsn2 ??= new();
        set => garSsn2 = value;
      }

      /// <summary>
      /// A value of GarSsn4.
      /// </summary>
      [JsonPropertyName("garSsn4")]
      public WorkArea GarSsn4
      {
        get => garSsn4 ??= new();
        set => garSsn4 = value;
      }

      /// <summary>
      /// A value of GarKscares.
      /// </summary>
      [JsonPropertyName("garKscares")]
      public Common GarKscares
      {
        get => garKscares ??= new();
        set => garKscares = value;
      }

      /// <summary>
      /// A value of GarKanpay.
      /// </summary>
      [JsonPropertyName("garKanpay")]
      public Common GarKanpay
      {
        get => garKanpay ??= new();
        set => garKanpay = value;
      }

      /// <summary>
      /// A value of GarCse.
      /// </summary>
      [JsonPropertyName("garCse")]
      public Common GarCse
      {
        get => garCse ??= new();
        set => garCse = value;
      }

      /// <summary>
      /// A value of GarAe.
      /// </summary>
      [JsonPropertyName("garAe")]
      public Common GarAe
      {
        get => garAe ??= new();
        set => garAe = value;
      }

      /// <summary>
      /// A value of GarDbOccurrence.
      /// </summary>
      [JsonPropertyName("garDbOccurrence")]
      public Common GarDbOccurrence
      {
        get => garDbOccurrence ??= new();
        set => garDbOccurrence = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common garCommon;
      private CsePersonsWorkSet garCsePersonsWorkSet;
      private WorkArea garSsn3;
      private WorkArea garSsn2;
      private WorkArea garSsn4;
      private Common garKscares;
      private Common garKanpay;
      private Common garCse;
      private Common garAe;
      private Common garDbOccurrence;
    }

    /// <summary>
    /// A value of ArOccur.
    /// </summary>
    [JsonPropertyName("arOccur")]
    public Common ArOccur
    {
      get => arOccur ??= new();
      set => arOccur = value;
    }

    /// <summary>
    /// A value of ApOccur.
    /// </summary>
    [JsonPropertyName("apOccur")]
    public Common ApOccur
    {
      get => apOccur ??= new();
      set => apOccur = value;
    }

    /// <summary>
    /// A value of NextKeyAp.
    /// </summary>
    [JsonPropertyName("nextKeyAp")]
    public CsePersonsWorkSet NextKeyAp
    {
      get => nextKeyAp ??= new();
      set => nextKeyAp = value;
    }

    /// <summary>
    /// A value of NextKeyAr.
    /// </summary>
    [JsonPropertyName("nextKeyAr")]
    public CsePersonsWorkSet NextKeyAr
    {
      get => nextKeyAr ??= new();
      set => nextKeyAr = value;
    }

    /// <summary>
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ar.
    /// </summary>
    [JsonIgnore]
    public Array<ArGroup> Ar => ar ??= new(ArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ar for json serialization.
    /// </summary>
    [JsonPropertyName("ar")]
    [Computed]
    public IList<ArGroup> Ar_Json
    {
      get => ar;
      set => Ar.Assign(value);
    }

    private Common arOccur;
    private Common apOccur;
    private CsePersonsWorkSet nextKeyAp;
    private CsePersonsWorkSet nextKeyAr;
    private Array<ApGroup> ap;
    private Array<ArGroup> ar;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
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
      /// A value of Gcse.
      /// </summary>
      [JsonPropertyName("gcse")]
      public Common Gcse
      {
        get => gcse ??= new();
        set => gcse = value;
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
      private Common gcse;
      private Common gae;
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

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private Array<GroupGroup> group;
    private AbendData abendData;
  }
#endregion
}
