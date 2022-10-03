// Program: SP_CAB_NATE_GET_NARR_DETAILS, ID: 370960544, model: 746.
// Short name: SWE02040
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_CAB_NATE_GET_NARR_DETAILS.
/// </summary>
[Serializable]
public partial class SpCabNateGetNarrDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CAB_NATE_GET_NARR_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCabNateGetNarrDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCabNateGetNarrDetails.
  /// </summary>
  public SpCabNateGetNarrDetails(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------------------------------------------------
    //        M A I N T E N A N C E   L O G
    // --------------------------------------------------------------------------------------
    // Date   Developer PR#/WR#  Description
    // 06/06/00  SWSRCHF  000170   Initial development
    // 09/20/00  SWSRCHF H00104020 Replaced the comparison of a literal to a 
    // text field
    // --------------------------------------------------------------------------------------
    for(import.HiddenKeys.Index = 0; import.HiddenKeys.Index < import
      .HiddenKeys.Count; ++import.HiddenKeys.Index)
    {
      if (!import.HiddenKeys.CheckSize())
      {
        break;
      }

      export.HiddenKeys.Index = import.HiddenKeys.Index;
      export.HiddenKeys.CheckSize();

      export.HiddenKeys.Update.HiddenKey.
        Assign(import.HiddenKeys.Item.HiddenKey);
    }

    import.HiddenKeys.CheckIndex();
    local.Enf.Flag = "N";
    local.Est.Flag = "N";
    local.Common.Flag = "N";
    local.Med.Flag = "N";
    local.Pat.Flag = "N";
    local.ModRevEnf.Flag = "N";
    local.ModRevEst.Flag = "N";
    local.ModRev.Flag = "N";
    local.ModRevMed.Flag = "N";
    local.ModRevPat.Flag = "N";

    export.HiddenKeys.Index = import.Hidden.PageNumber - 1;
    export.HiddenKeys.CheckSize();

    export.Detail.Index = -1;

    if (import.HeaderInfrastructure.SystemGeneratedIdentifier == 0)
    {
      // *** No Infrastructure record passed
    }
    else
    {
      // *** Infrastructure record passed
      local.WorkNarrativeDetail.InfrastructureId =
        import.HeaderInfrastructure.SystemGeneratedIdentifier;

      // *** 09/20/00 SWSRCHF
      // *** Set the literal values
      // *** start
      local.WorkNarrativeDetailWorkset.Text10 = "LOCATE --";
      local.WorkNarrativeDetailWorkset.Text11 = "MEDICAL --";
      local.WorkNarrativeDetailWorkset.Text13 = "PATERNITY --";
      local.WorkNarrativeDetailWorkset.Text15 = "ENFORCEMENT --";
      local.WorkNarrativeDetailWorkset.Text17 = "ESTABLISHMENT --";
      local.WorkNarrativeDetailWorkset.Text21 = "MOD REVIEW LOCATE --";
      local.WorkNarrativeDetailWorkset.Text22 = "MOD REVIEW MEDICAL --";
      local.WorkNarrativeDetailWorkset.Text24 = "MOD REVIEW PATERNITY --";
      local.WorkNarrativeDetailWorkset.Text26 = "MOD REVIEW ENFORCEMENT --";
      local.WorkNarrativeDetailWorkset.Text28 = "MOD REVIEW ESTABLISHMENT --";

      // *** end
      // *** Set the literal values
      // *** 09/20/00 SWSRCHF
      foreach(var item in ReadNarrativeDetail())
      {
        ++export.Detail.Index;
        export.Detail.CheckSize();

        if (export.Detail.Index >= Export.DetailGroup.Capacity)
        {
          if (export.HiddenKeys.Index + 1 < Export.HiddenKeysGroup.Capacity)
          {
            ++export.HiddenKeys.Index;
            export.HiddenKeys.CheckSize();

            export.HiddenKeys.Update.HiddenKey.CreatedTimestamp =
              entities.ExistingNarrativeDetail.CreatedTimestamp;
            export.HiddenKeys.Update.HiddenKey.InfrastructureId =
              entities.ExistingNarrativeDetail.InfrastructureId;
            export.HiddenKeys.Update.HiddenKey.LineNumber =
              entities.ExistingNarrativeDetail.LineNumber;
          }
          else
          {
            ExitState = "SP0000_LIST_IS_FULL";
          }

          return;
        }

        export.Detail.Update.DtlNarrativeDetail.Assign(
          entities.ExistingNarrativeDetail);

        if (entities.ExistingNarrativeDetail.LineNumber != 1)
        {
          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 10,
            local.WorkNarrativeDetailWorkset.Text10))
          {
            if (AsChar(local.Common.Flag) == 'N')
            {
              local.Common.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 11, 58);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 11,
            local.WorkNarrativeDetailWorkset.Text11))
          {
            if (AsChar(local.Med.Flag) == 'N')
            {
              local.Med.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 12, 57);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 13,
            local.WorkNarrativeDetailWorkset.Text13))
          {
            if (AsChar(local.Pat.Flag) == 'N')
            {
              local.Pat.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 14, 55);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 15,
            local.WorkNarrativeDetailWorkset.Text15))
          {
            if (AsChar(local.Enf.Flag) == 'N')
            {
              local.Enf.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 16, 53);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 17,
            local.WorkNarrativeDetailWorkset.Text17))
          {
            if (AsChar(local.Est.Flag) == 'N')
            {
              local.Est.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 18, 51);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 21,
            local.WorkNarrativeDetailWorkset.Text21))
          {
            if (AsChar(local.ModRev.Flag) == 'N')
            {
              local.ModRev.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 22, 47);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 22,
            local.WorkNarrativeDetailWorkset.Text22))
          {
            if (AsChar(local.ModRevMed.Flag) == 'N')
            {
              local.ModRevMed.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 23, 46);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 24,
            local.WorkNarrativeDetailWorkset.Text24))
          {
            if (AsChar(local.ModRevPat.Flag) == 'N')
            {
              local.ModRevPat.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 25, 44);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 26,
            local.WorkNarrativeDetailWorkset.Text26))
          {
            if (AsChar(local.ModRevEnf.Flag) == 'N')
            {
              local.ModRevEnf.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 27, 42);
                
            }
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 28,
            local.WorkNarrativeDetailWorkset.Text28))
          {
            if (AsChar(local.ModRevEst.Flag) == 'N')
            {
              local.ModRevEst.Flag = "Y";
            }
            else
            {
              export.Detail.Update.DtlNarrativeDetail.NarrativeText =
                Substring(entities.ExistingNarrativeDetail.NarrativeText, 29, 40);
                
            }
          }
        }
        else
        {
          local.Enf.Flag = "N";
          local.Est.Flag = "N";
          local.Common.Flag = "N";
          local.Med.Flag = "N";
          local.Pat.Flag = "N";
          local.ModRevEnf.Flag = "N";
          local.ModRevEst.Flag = "N";
          local.ModRev.Flag = "N";
          local.ModRevMed.Flag = "N";
          local.ModRevPat.Flag = "N";

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 10,
            local.WorkNarrativeDetailWorkset.Text10))
          {
            local.Common.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 11,
            local.WorkNarrativeDetailWorkset.Text11))
          {
            local.Med.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 13,
            local.WorkNarrativeDetailWorkset.Text13))
          {
            local.Pat.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 15,
            local.WorkNarrativeDetailWorkset.Text15))
          {
            local.Enf.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 17,
            local.WorkNarrativeDetailWorkset.Text17))
          {
            local.Est.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 21,
            local.WorkNarrativeDetailWorkset.Text21))
          {
            local.ModRev.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 22,
            local.WorkNarrativeDetailWorkset.Text22))
          {
            local.ModRevMed.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 24,
            local.WorkNarrativeDetailWorkset.Text24))
          {
            local.ModRevPat.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 26,
            local.WorkNarrativeDetailWorkset.Text26))
          {
            local.ModRevEnf.Flag = "Y";
          }

          if (Equal(entities.ExistingNarrativeDetail.NarrativeText, 1, 28,
            local.WorkNarrativeDetailWorkset.Text28))
          {
            local.ModRevEst.Flag = "Y";
          }
        }
      }
    }
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.ExistingNarrativeDetail.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId1",
          local.WorkNarrativeDetail.InfrastructureId);
        db.SetDate(command, "date", import.Filter.Date.GetValueOrDefault());
        db.SetDateTime(
          command, "createdTimestamp",
          export.HiddenKeys.Item.HiddenKey.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "infrastructureId2",
          export.HiddenKeys.Item.HiddenKey.InfrastructureId);
        db.SetInt32(
          command, "lineNumber", export.HiddenKeys.Item.HiddenKey.LineNumber);
      },
      (db, reader) =>
      {
        entities.ExistingNarrativeDetail.InfrastructureId =
          db.GetInt32(reader, 0);
        entities.ExistingNarrativeDetail.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingNarrativeDetail.CaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingNarrativeDetail.NarrativeText =
          db.GetNullableString(reader, 3);
        entities.ExistingNarrativeDetail.LineNumber = db.GetInt32(reader, 4);
        entities.ExistingNarrativeDetail.Populated = true;

        return true;
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
    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of HiddenKey.
      /// </summary>
      [JsonPropertyName("hiddenKey")]
      public NarrativeDetail HiddenKey
      {
        get => hiddenKey ??= new();
        set => hiddenKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private NarrativeDetail hiddenKey;
    }

    /// <summary>
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
    }

    /// <summary>
    /// A value of HeaderInfrastructure.
    /// </summary>
    [JsonPropertyName("headerInfrastructure")]
    public Infrastructure HeaderInfrastructure
    {
      get => headerInfrastructure ??= new();
      set => headerInfrastructure = value;
    }

    /// <summary>
    /// A value of HeaderDateWorkArea.
    /// </summary>
    [JsonPropertyName("headerDateWorkArea")]
    public DateWorkArea HeaderDateWorkArea
    {
      get => headerDateWorkArea ??= new();
      set => headerDateWorkArea = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public Standard Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public DateWorkArea Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    private Array<HiddenKeysGroup> hiddenKeys;
    private Infrastructure headerInfrastructure;
    private DateWorkArea headerDateWorkArea;
    private Standard hidden;
    private DateWorkArea filter;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A DetailGroup group.</summary>
    [Serializable]
    public class DetailGroup
    {
      /// <summary>
      /// A value of DtlCommon.
      /// </summary>
      [JsonPropertyName("dtlCommon")]
      public Common DtlCommon
      {
        get => dtlCommon ??= new();
        set => dtlCommon = value;
      }

      /// <summary>
      /// A value of DtlNarrativeDetail.
      /// </summary>
      [JsonPropertyName("dtlNarrativeDetail")]
      public NarrativeDetail DtlNarrativeDetail
      {
        get => dtlNarrativeDetail ??= new();
        set => dtlNarrativeDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private Common dtlCommon;
      private NarrativeDetail dtlNarrativeDetail;
    }

    /// <summary>A HiddenKeysGroup group.</summary>
    [Serializable]
    public class HiddenKeysGroup
    {
      /// <summary>
      /// A value of HiddenKey.
      /// </summary>
      [JsonPropertyName("hiddenKey")]
      public NarrativeDetail HiddenKey
      {
        get => hiddenKey ??= new();
        set => hiddenKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private NarrativeDetail hiddenKey;
    }

    /// <summary>
    /// Gets a value of Detail.
    /// </summary>
    [JsonIgnore]
    public Array<DetailGroup> Detail => detail ??= new(DetailGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Detail for json serialization.
    /// </summary>
    [JsonPropertyName("detail")]
    [Computed]
    public IList<DetailGroup> Detail_Json
    {
      get => detail;
      set => Detail.Assign(value);
    }

    /// <summary>
    /// Gets a value of HiddenKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenKeysGroup> HiddenKeys => hiddenKeys ??= new(
      HiddenKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenKeys")]
    [Computed]
    public IList<HiddenKeysGroup> HiddenKeys_Json
    {
      get => hiddenKeys;
      set => HiddenKeys.Assign(value);
    }

    private Array<DetailGroup> detail;
    private Array<HiddenKeysGroup> hiddenKeys;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of WorkNarrativeDetailWorkset.
    /// </summary>
    [JsonPropertyName("workNarrativeDetailWorkset")]
    public NarrativeDetailWorkset WorkNarrativeDetailWorkset
    {
      get => workNarrativeDetailWorkset ??= new();
      set => workNarrativeDetailWorkset = value;
    }

    /// <summary>
    /// A value of WorkNarrativeDetail.
    /// </summary>
    [JsonPropertyName("workNarrativeDetail")]
    public NarrativeDetail WorkNarrativeDetail
    {
      get => workNarrativeDetail ??= new();
      set => workNarrativeDetail = value;
    }

    /// <summary>
    /// A value of WorkInfrastructure.
    /// </summary>
    [JsonPropertyName("workInfrastructure")]
    public Infrastructure WorkInfrastructure
    {
      get => workInfrastructure ??= new();
      set => workInfrastructure = value;
    }

    /// <summary>
    /// A value of ModRevPat.
    /// </summary>
    [JsonPropertyName("modRevPat")]
    public Common ModRevPat
    {
      get => modRevPat ??= new();
      set => modRevPat = value;
    }

    /// <summary>
    /// A value of ModRevEnf.
    /// </summary>
    [JsonPropertyName("modRevEnf")]
    public Common ModRevEnf
    {
      get => modRevEnf ??= new();
      set => modRevEnf = value;
    }

    /// <summary>
    /// A value of ModRevMed.
    /// </summary>
    [JsonPropertyName("modRevMed")]
    public Common ModRevMed
    {
      get => modRevMed ??= new();
      set => modRevMed = value;
    }

    /// <summary>
    /// A value of ModRevEst.
    /// </summary>
    [JsonPropertyName("modRevEst")]
    public Common ModRevEst
    {
      get => modRevEst ??= new();
      set => modRevEst = value;
    }

    /// <summary>
    /// A value of ModRev.
    /// </summary>
    [JsonPropertyName("modRev")]
    public Common ModRev
    {
      get => modRev ??= new();
      set => modRev = value;
    }

    /// <summary>
    /// A value of Pat.
    /// </summary>
    [JsonPropertyName("pat")]
    public Common Pat
    {
      get => pat ??= new();
      set => pat = value;
    }

    /// <summary>
    /// A value of Enf.
    /// </summary>
    [JsonPropertyName("enf")]
    public Common Enf
    {
      get => enf ??= new();
      set => enf = value;
    }

    /// <summary>
    /// A value of Med.
    /// </summary>
    [JsonPropertyName("med")]
    public Common Med
    {
      get => med ??= new();
      set => med = value;
    }

    /// <summary>
    /// A value of Est.
    /// </summary>
    [JsonPropertyName("est")]
    public Common Est
    {
      get => est ??= new();
      set => est = value;
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

    private NarrativeDetailWorkset workNarrativeDetailWorkset;
    private NarrativeDetail workNarrativeDetail;
    private Infrastructure workInfrastructure;
    private Common modRevPat;
    private Common modRevEnf;
    private Common modRevMed;
    private Common modRevEst;
    private Common modRev;
    private Common pat;
    private Common enf;
    private Common med;
    private Common est;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingEventDetail.
    /// </summary>
    [JsonPropertyName("existingEventDetail")]
    public EventDetail ExistingEventDetail
    {
      get => existingEventDetail ??= new();
      set => existingEventDetail = value;
    }

    /// <summary>
    /// A value of ExistingEvent.
    /// </summary>
    [JsonPropertyName("existingEvent")]
    public Event1 ExistingEvent
    {
      get => existingEvent ??= new();
      set => existingEvent = value;
    }

    /// <summary>
    /// A value of ExistingInfrastructure.
    /// </summary>
    [JsonPropertyName("existingInfrastructure")]
    public Infrastructure ExistingInfrastructure
    {
      get => existingInfrastructure ??= new();
      set => existingInfrastructure = value;
    }

    /// <summary>
    /// A value of ExistingNarrativeDetail.
    /// </summary>
    [JsonPropertyName("existingNarrativeDetail")]
    public NarrativeDetail ExistingNarrativeDetail
    {
      get => existingNarrativeDetail ??= new();
      set => existingNarrativeDetail = value;
    }

    private EventDetail existingEventDetail;
    private Event1 existingEvent;
    private Infrastructure existingInfrastructure;
    private NarrativeDetail existingNarrativeDetail;
  }
#endregion
}
