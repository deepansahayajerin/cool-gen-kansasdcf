// Program: SI_ALTS_BUILD_ALIAS_AND_SSN_2, ID: 373488453, model: 746.
// Short name: SWE00407
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_ALTS_BUILD_ALIAS_AND_SSN_2.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiAltsBuildAliasAndSsn2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ALTS_BUILD_ALIAS_AND_SSN_2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAltsBuildAliasAndSsn2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAltsBuildAliasAndSsn2.
  /// </summary>
  public SiAltsBuildAliasAndSsn2(IContext context, Import import, Export export):
    
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
    //   Date                  Developer	           Description
    // 05/14/2002       Vithal Madhira
    // This is a copy of 'SI_ALTS_BUILD_ALIAS_AND_SSN'. Since the screen is 
    // redesigned to display 3 records for AP/AR/CH instead of 6 , I need to
    // read 3 alias records and accordingly I need to change the groupview size
    // from 6 to 3. Since  'SI_ALTS_BUILD_ALIAS_AND_SSN'  used by several
    // programs I created a new CAB.
    // --------------------------------------------------------------------------------------
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
      UseEabAltsReadAlias1();

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
              export.Ap.Update.GapFa.Flag = local.Group.Item.Gfacts.Flag;
              export.Ap.Update.GapActiveOnKscares.Flag =
                local.Group.Item.GactiveOnKscares.Flag;
              export.Ap.Update.GapActiveOnKanpay.Flag =
                local.Group.Item.GactiveOnKanpay.Flag;
              export.Ap.Update.GapActiveOnCse.Flag =
                local.Group.Item.GactiveOnCse.Flag;
              export.Ap.Update.GapActiveOnAe.Flag =
                local.Group.Item.GactiveOnAe.Flag;
              export.Ap.Update.GapActiveOnFa.Flag =
                local.Group.Item.GactiveOnFacts.Flag;
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
        goto Test;
      }

      UseEabAltsReadAlias2();

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
              export.Ar.Update.GarFa.Flag = local.Group.Item.Gfacts.Flag;
              export.Ar.Update.GarActiveOnKscares.Flag =
                local.Group.Item.GactiveOnKscares.Flag;
              export.Ar.Update.GarActiveOnKanpay.Flag =
                local.Group.Item.GactiveOnKanpay.Flag;
              export.Ar.Update.GarActiveOnCse.Flag =
                local.Group.Item.GactiveOnCse.Flag;
              export.Ar.Update.GarActiveOnAe.Flag =
                local.Group.Item.GactiveOnAe.Flag;
              export.Ar.Update.GarActiveOnFa.Flag =
                local.Group.Item.GactiveOnFacts.Flag;
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

Test:

    local.Group.Count = 0;

    // *********************************************
    // Read for the CHILD
    // *********************************************
    if (!IsEmpty(import.Ch1.Number))
    {
      if (CharAt(import.Ch1.Number, 10) == 'O')
      {
        return;
      }

      UseEabAltsReadAlias3();

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

              export.Ch.Index = local.Group.Index;
              export.Ch.CheckSize();

              export.Ch.Update.GchCsePersonsWorkSet.Assign(local.Group.Item.G);

              // ---------------------------------------------
              // 02/11/99 W.Campbell - Fixed the target on
              // the following move statement which was giving
              // consistency check warnings because it
              // would not move anything. i.e. there were no
              // attributes in common in the source and
              // target views.
              // ---------------------------------------------
              // 	
              export.Ch.Update.GchAe.Flag = local.Group.Item.Gae.Flag;
              export.Ch.Update.GchCse.Flag = local.Group.Item.Gcse.Flag;
              export.Ch.Update.GchKanpay.Flag = local.Group.Item.Gkanpay.Flag;
              export.Ch.Update.GchKscares.Flag = local.Group.Item.Gkscares.Flag;
              export.Ch.Update.GchFa.Flag = local.Group.Item.Gfacts.Flag;
              export.Ch.Update.GchActiveOnKscares.Flag =
                local.Group.Item.GactiveOnKscares.Flag;
              export.Ch.Update.GchActiveOnKanpay.Flag =
                local.Group.Item.GactiveOnKanpay.Flag;
              export.Ch.Update.GchActiveOnCse.Flag =
                local.Group.Item.GactiveOnCse.Flag;
              export.Ch.Update.GchActiveOnAe.Flag =
                local.Group.Item.GactiveOnAe.Flag;
              export.Ch.Update.GchActiveOnFa.Flag =
                local.Group.Item.GactiveOnFacts.Flag;
              export.Ch.Update.GchSsn3.Text3 =
                Substring(local.Group.Item.G.Ssn, 1, 3);
              export.Ch.Update.GchSsn2.Text3 =
                Substring(local.Group.Item.G.Ssn, 4, 2);
              export.Ch.Update.GchSsn4.Text4 =
                Substring(local.Group.Item.G.Ssn, 6, 4);
              export.Ch.Update.GchDbOccurrence.Flag = "Y";
            }

            local.Group.CheckIndex();
            export.ChOccur.Flag = "Y";
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

  private static void MoveAliasesToGroup(EabAltsReadAlias.Export.
    AliasesGroup source, Local.GroupGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
    target.Gfacts.Flag = source.Gfacts.Flag;
    target.GactiveOnKscares.Flag = source.GactiveOnKscares.Flag;
    target.GactiveOnKanpay.Flag = source.GactiveOnKanpay.Flag;
    target.GactiveOnCse.Flag = source.GactiveOnCse.Flag;
    target.GactiveOnAe.Flag = source.GactiveOnAe.Flag;
    target.GactiveOnFacts.Flag = source.GactiveOnFacts.Flag;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private static void MoveGroupToAliases(Local.GroupGroup source,
    EabAltsReadAlias.Export.AliasesGroup target)
  {
    target.G.Assign(source.G);
    target.Gkscares.Flag = source.Gkscares.Flag;
    target.Gkanpay.Flag = source.Gkanpay.Flag;
    target.Gcse.Flag = source.Gcse.Flag;
    target.Gae.Flag = source.Gae.Flag;
    target.Gfacts.Flag = source.Gfacts.Flag;
    target.GactiveOnKscares.Flag = source.GactiveOnKscares.Flag;
    target.GactiveOnKanpay.Flag = source.GactiveOnKanpay.Flag;
    target.GactiveOnCse.Flag = source.GactiveOnCse.Flag;
    target.GactiveOnAe.Flag = source.GactiveOnAe.Flag;
    target.GactiveOnFacts.Flag = source.GactiveOnFacts.Flag;
  }

  private void UseEabAltsReadAlias1()
  {
    var useImport = new EabAltsReadAlias.Import();
    var useExport = new EabAltsReadAlias.Export();

    MoveCsePersonsWorkSet(import.Ap1, useImport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);
    useExport.NextKey.UniqueKey = export.NextKeyAp.UniqueKey;
    local.Group.CopyTo(useExport.Aliases, MoveGroupToAliases);

    Call(EabAltsReadAlias.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.NextKeyAp.UniqueKey = useExport.NextKey.UniqueKey;
    useExport.Aliases.CopyTo(local.Group, MoveAliasesToGroup);
  }

  private void UseEabAltsReadAlias2()
  {
    var useImport = new EabAltsReadAlias.Import();
    var useExport = new EabAltsReadAlias.Export();

    MoveCsePersonsWorkSet(import.Ar1, useImport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);
    useExport.NextKey.UniqueKey = export.NextKeyAr.UniqueKey;
    local.Group.CopyTo(useExport.Aliases, MoveGroupToAliases);

    Call(EabAltsReadAlias.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.NextKeyAr.UniqueKey = useExport.NextKey.UniqueKey;
    useExport.Aliases.CopyTo(local.Group, MoveAliasesToGroup);
  }

  private void UseEabAltsReadAlias3()
  {
    var useImport = new EabAltsReadAlias.Import();
    var useExport = new EabAltsReadAlias.Export();

    MoveCsePersonsWorkSet(import.Ch1, useImport.CsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);
    useExport.NextKey.UniqueKey = export.NextKeyCh.UniqueKey;
    local.Group.CopyTo(useExport.Aliases, MoveGroupToAliases);

    Call(EabAltsReadAlias.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    export.NextKeyCh.UniqueKey = useExport.NextKey.UniqueKey;
    useExport.Aliases.CopyTo(local.Group, MoveAliasesToGroup);
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

    /// <summary>
    /// A value of Ch1.
    /// </summary>
    [JsonPropertyName("ch1")]
    public CsePersonsWorkSet Ch1
    {
      get => ch1 ??= new();
      set => ch1 = value;
    }

    private CsePersonsWorkSet ap1;
    private CsePersonsWorkSet ar1;
    private CsePersonsWorkSet ch1;
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
      /// A value of GapFa.
      /// </summary>
      [JsonPropertyName("gapFa")]
      public Common GapFa
      {
        get => gapFa ??= new();
        set => gapFa = value;
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

      /// <summary>
      /// A value of GapActiveOnKscares.
      /// </summary>
      [JsonPropertyName("gapActiveOnKscares")]
      public Common GapActiveOnKscares
      {
        get => gapActiveOnKscares ??= new();
        set => gapActiveOnKscares = value;
      }

      /// <summary>
      /// A value of GapActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("gapActiveOnKanpay")]
      public Common GapActiveOnKanpay
      {
        get => gapActiveOnKanpay ??= new();
        set => gapActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of GapActiveOnCse.
      /// </summary>
      [JsonPropertyName("gapActiveOnCse")]
      public Common GapActiveOnCse
      {
        get => gapActiveOnCse ??= new();
        set => gapActiveOnCse = value;
      }

      /// <summary>
      /// A value of GapActiveOnAe.
      /// </summary>
      [JsonPropertyName("gapActiveOnAe")]
      public Common GapActiveOnAe
      {
        get => gapActiveOnAe ??= new();
        set => gapActiveOnAe = value;
      }

      /// <summary>
      /// A value of GapActiveOnFa.
      /// </summary>
      [JsonPropertyName("gapActiveOnFa")]
      public Common GapActiveOnFa
      {
        get => gapActiveOnFa ??= new();
        set => gapActiveOnFa = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common gapCommon;
      private CsePersonsWorkSet gapCsePersonsWorkSet;
      private WorkArea gapSsn3;
      private WorkArea gapSsn2;
      private WorkArea gapSsn4;
      private Common gapKscares;
      private Common gapKanpay;
      private Common gapCse;
      private Common gapAe;
      private Common gapFa;
      private Common gapDbOccurrence;
      private Common gapActiveOnKscares;
      private Common gapActiveOnKanpay;
      private Common gapActiveOnCse;
      private Common gapActiveOnAe;
      private Common gapActiveOnFa;
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
      /// A value of GarFa.
      /// </summary>
      [JsonPropertyName("garFa")]
      public Common GarFa
      {
        get => garFa ??= new();
        set => garFa = value;
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

      /// <summary>
      /// A value of GarActiveOnKscares.
      /// </summary>
      [JsonPropertyName("garActiveOnKscares")]
      public Common GarActiveOnKscares
      {
        get => garActiveOnKscares ??= new();
        set => garActiveOnKscares = value;
      }

      /// <summary>
      /// A value of GarActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("garActiveOnKanpay")]
      public Common GarActiveOnKanpay
      {
        get => garActiveOnKanpay ??= new();
        set => garActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of GarActiveOnCse.
      /// </summary>
      [JsonPropertyName("garActiveOnCse")]
      public Common GarActiveOnCse
      {
        get => garActiveOnCse ??= new();
        set => garActiveOnCse = value;
      }

      /// <summary>
      /// A value of GarActiveOnAe.
      /// </summary>
      [JsonPropertyName("garActiveOnAe")]
      public Common GarActiveOnAe
      {
        get => garActiveOnAe ??= new();
        set => garActiveOnAe = value;
      }

      /// <summary>
      /// A value of GarActiveOnFa.
      /// </summary>
      [JsonPropertyName("garActiveOnFa")]
      public Common GarActiveOnFa
      {
        get => garActiveOnFa ??= new();
        set => garActiveOnFa = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common garCommon;
      private CsePersonsWorkSet garCsePersonsWorkSet;
      private WorkArea garSsn3;
      private WorkArea garSsn2;
      private WorkArea garSsn4;
      private Common garKscares;
      private Common garKanpay;
      private Common garCse;
      private Common garAe;
      private Common garFa;
      private Common garDbOccurrence;
      private Common garActiveOnKscares;
      private Common garActiveOnKanpay;
      private Common garActiveOnCse;
      private Common garActiveOnAe;
      private Common garActiveOnFa;
    }

    /// <summary>A ChGroup group.</summary>
    [Serializable]
    public class ChGroup
    {
      /// <summary>
      /// A value of GchCommon.
      /// </summary>
      [JsonPropertyName("gchCommon")]
      public Common GchCommon
      {
        get => gchCommon ??= new();
        set => gchCommon = value;
      }

      /// <summary>
      /// A value of GchCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gchCsePersonsWorkSet")]
      public CsePersonsWorkSet GchCsePersonsWorkSet
      {
        get => gchCsePersonsWorkSet ??= new();
        set => gchCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GchSsn3.
      /// </summary>
      [JsonPropertyName("gchSsn3")]
      public WorkArea GchSsn3
      {
        get => gchSsn3 ??= new();
        set => gchSsn3 = value;
      }

      /// <summary>
      /// A value of GchSsn2.
      /// </summary>
      [JsonPropertyName("gchSsn2")]
      public WorkArea GchSsn2
      {
        get => gchSsn2 ??= new();
        set => gchSsn2 = value;
      }

      /// <summary>
      /// A value of GchSsn4.
      /// </summary>
      [JsonPropertyName("gchSsn4")]
      public WorkArea GchSsn4
      {
        get => gchSsn4 ??= new();
        set => gchSsn4 = value;
      }

      /// <summary>
      /// A value of GchKscares.
      /// </summary>
      [JsonPropertyName("gchKscares")]
      public Common GchKscares
      {
        get => gchKscares ??= new();
        set => gchKscares = value;
      }

      /// <summary>
      /// A value of GchKanpay.
      /// </summary>
      [JsonPropertyName("gchKanpay")]
      public Common GchKanpay
      {
        get => gchKanpay ??= new();
        set => gchKanpay = value;
      }

      /// <summary>
      /// A value of GchCse.
      /// </summary>
      [JsonPropertyName("gchCse")]
      public Common GchCse
      {
        get => gchCse ??= new();
        set => gchCse = value;
      }

      /// <summary>
      /// A value of GchAe.
      /// </summary>
      [JsonPropertyName("gchAe")]
      public Common GchAe
      {
        get => gchAe ??= new();
        set => gchAe = value;
      }

      /// <summary>
      /// A value of GchFa.
      /// </summary>
      [JsonPropertyName("gchFa")]
      public Common GchFa
      {
        get => gchFa ??= new();
        set => gchFa = value;
      }

      /// <summary>
      /// A value of GchDbOccurrence.
      /// </summary>
      [JsonPropertyName("gchDbOccurrence")]
      public Common GchDbOccurrence
      {
        get => gchDbOccurrence ??= new();
        set => gchDbOccurrence = value;
      }

      /// <summary>
      /// A value of GchActiveOnKscares.
      /// </summary>
      [JsonPropertyName("gchActiveOnKscares")]
      public Common GchActiveOnKscares
      {
        get => gchActiveOnKscares ??= new();
        set => gchActiveOnKscares = value;
      }

      /// <summary>
      /// A value of GchActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("gchActiveOnKanpay")]
      public Common GchActiveOnKanpay
      {
        get => gchActiveOnKanpay ??= new();
        set => gchActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of GchActiveOnCse.
      /// </summary>
      [JsonPropertyName("gchActiveOnCse")]
      public Common GchActiveOnCse
      {
        get => gchActiveOnCse ??= new();
        set => gchActiveOnCse = value;
      }

      /// <summary>
      /// A value of GchActiveOnAe.
      /// </summary>
      [JsonPropertyName("gchActiveOnAe")]
      public Common GchActiveOnAe
      {
        get => gchActiveOnAe ??= new();
        set => gchActiveOnAe = value;
      }

      /// <summary>
      /// A value of GchActiveOnFa.
      /// </summary>
      [JsonPropertyName("gchActiveOnFa")]
      public Common GchActiveOnFa
      {
        get => gchActiveOnFa ??= new();
        set => gchActiveOnFa = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common gchCommon;
      private CsePersonsWorkSet gchCsePersonsWorkSet;
      private WorkArea gchSsn3;
      private WorkArea gchSsn2;
      private WorkArea gchSsn4;
      private Common gchKscares;
      private Common gchKanpay;
      private Common gchCse;
      private Common gchAe;
      private Common gchFa;
      private Common gchDbOccurrence;
      private Common gchActiveOnKscares;
      private Common gchActiveOnKanpay;
      private Common gchActiveOnCse;
      private Common gchActiveOnAe;
      private Common gchActiveOnFa;
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

    /// <summary>
    /// Gets a value of Ch.
    /// </summary>
    [JsonIgnore]
    public Array<ChGroup> Ch => ch ??= new(ChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ch for json serialization.
    /// </summary>
    [JsonPropertyName("ch")]
    [Computed]
    public IList<ChGroup> Ch_Json
    {
      get => ch;
      set => Ch.Assign(value);
    }

    /// <summary>
    /// A value of NextKeyCh.
    /// </summary>
    [JsonPropertyName("nextKeyCh")]
    public CsePersonsWorkSet NextKeyCh
    {
      get => nextKeyCh ??= new();
      set => nextKeyCh = value;
    }

    /// <summary>
    /// A value of ChOccur.
    /// </summary>
    [JsonPropertyName("chOccur")]
    public Common ChOccur
    {
      get => chOccur ??= new();
      set => chOccur = value;
    }

    private Common arOccur;
    private Common apOccur;
    private CsePersonsWorkSet nextKeyAp;
    private CsePersonsWorkSet nextKeyAr;
    private Array<ApGroup> ap;
    private Array<ArGroup> ar;
    private Array<ChGroup> ch;
    private CsePersonsWorkSet nextKeyCh;
    private Common chOccur;
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

      /// <summary>
      /// A value of Gfacts.
      /// </summary>
      [JsonPropertyName("gfacts")]
      public Common Gfacts
      {
        get => gfacts ??= new();
        set => gfacts = value;
      }

      /// <summary>
      /// A value of GactiveOnKscares.
      /// </summary>
      [JsonPropertyName("gactiveOnKscares")]
      public Common GactiveOnKscares
      {
        get => gactiveOnKscares ??= new();
        set => gactiveOnKscares = value;
      }

      /// <summary>
      /// A value of GactiveOnKanpay.
      /// </summary>
      [JsonPropertyName("gactiveOnKanpay")]
      public Common GactiveOnKanpay
      {
        get => gactiveOnKanpay ??= new();
        set => gactiveOnKanpay = value;
      }

      /// <summary>
      /// A value of GactiveOnCse.
      /// </summary>
      [JsonPropertyName("gactiveOnCse")]
      public Common GactiveOnCse
      {
        get => gactiveOnCse ??= new();
        set => gactiveOnCse = value;
      }

      /// <summary>
      /// A value of GactiveOnAe.
      /// </summary>
      [JsonPropertyName("gactiveOnAe")]
      public Common GactiveOnAe
      {
        get => gactiveOnAe ??= new();
        set => gactiveOnAe = value;
      }

      /// <summary>
      /// A value of GactiveOnFacts.
      /// </summary>
      [JsonPropertyName("gactiveOnFacts")]
      public Common GactiveOnFacts
      {
        get => gactiveOnFacts ??= new();
        set => gactiveOnFacts = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePersonsWorkSet g;
      private Common gkscares;
      private Common gkanpay;
      private Common gcse;
      private Common gae;
      private Common gfacts;
      private Common gactiveOnKscares;
      private Common gactiveOnKanpay;
      private Common gactiveOnCse;
      private Common gactiveOnAe;
      private Common gactiveOnFacts;
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
