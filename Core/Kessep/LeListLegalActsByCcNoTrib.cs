// Program: LE_LIST_LEGAL_ACTS_BY_CC_NO_TRIB, ID: 372002858, model: 746.
// Short name: SWE00806
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
/// A program: LE_LIST_LEGAL_ACTS_BY_CC_NO_TRIB.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will list information on the List Legal Actions by Court 
/// Case Number screen.
/// </para>
/// </summary>
[Serializable]
public partial class LeListLegalActsByCcNoTrib: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_LEGAL_ACTS_BY_CC_NO_TRIB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListLegalActsByCcNoTrib(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListLegalActsByCcNoTrib.
  /// </summary>
  public LeListLegalActsByCcNoTrib(IContext context, Import import,
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
    // --------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // 05/09/95  Dave Allen			Initial Code
    // 04/30/97  Govind	267		Added flows to SERV, LRES and HEAR
    // 					Fixed view overflow problem
    // 01/13/99  P. Sharp			Changes based on Phase II assessment
    // 04/02/02  GVandy	PR# 138221	Read for end dated code values when 
    // retrieving
    // 					action taken description.
    // 12/23/02  GVandy	WR10492		Read and export attribute system_gen_ind.
    // --------------------------------------------------------------------------------------------
    export.SearchLegalAction.Assign(import.SearchLegalAction);
    MoveFips(import.SearchFips, export.SearchFips);
    local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
    local.TribPopulatedIndicator.Flag = "N";

    // ------------------------------------------------------------
    // If the Court Case Number was entered, Read by Court Case
    // Number, otherwise read by Standard Number
    // ------------------------------------------------------------
    export.LegalAction.Index = -1;

    if (!IsEmpty(import.SearchLegalAction.CourtCaseNumber) || IsEmpty
      (import.SearchLegalAction.StandardNumber))
    {
      // --- If court case number is supplied, use that for search. If the court
      // case number is not supplied, then if standard number is supplied, use
      // the standard number for search. Otherwise look for blank court case
      // number.
      foreach(var item in ReadLegalActionTribunal1())
      {
        if (Equal(import.SearchLegalAction.CreatedTstamp,
          local.Null1.CreatedTstamp))
        {
          // *** Continue on need all legal actions
        }
        else if (Lt(entities.LegalAction1.CreatedTstamp,
          import.SearchLegalAction.CreatedTstamp))
        {
          // *** Continue on need all the legal action less than the import 
          // seach timestamp
        }
        else
        {
          continue;
        }

        if (ReadFips())
        {
          if (!Equal(entities.Fips.StateAbbreviation,
            import.SearchFips.StateAbbreviation) || !
            Equal(entities.Fips.CountyAbbreviation,
            import.SearchFips.CountyAbbreviation))
          {
            continue;
          }
        }
        else if (entities.Tribunal.Identifier != import
          .SearchTribunal.Identifier)
        {
          continue;
        }

        // ------------------------------------------------------------
        // If the Classification has been entered, display only those
        // Legal Actions with that Classification for the specified
        // Court Case Number. If the Classification has not been
        // entered, display Legal Actions with any Classification for
        // the specified Court Case Number.
        // ------------------------------------------------------------
        if (!IsEmpty(import.SearchLegalAction.Classification))
        {
          if (AsChar(entities.LegalAction1.Classification) == AsChar
            (import.SearchLegalAction.Classification))
          {
            if (export.LegalAction.Index + 1 >= Export
              .LegalActionGroup.Capacity)
            {
              return;
            }

            ++export.LegalAction.Index;
            export.LegalAction.CheckSize();

            export.LegalAction.Update.LegalAction1.
              Assign(entities.LegalAction1);
          }
          else
          {
            continue;
          }
        }
        else
        {
          if (export.LegalAction.Index + 1 >= Export.LegalActionGroup.Capacity)
          {
            return;
          }

          ++export.LegalAction.Index;
          export.LegalAction.CheckSize();

          export.LegalAction.Update.LegalAction1.Assign(entities.LegalAction1);
        }

        if (ReadLegalActionAppeal())
        {
          export.LegalAction.Update.LappInd.Flag = "Y";
        }
        else
        {
          export.LegalAction.Update.LappInd.Flag = "";
        }

        UseLeGetActionTakenDescription();

        // ------------------------------------------------------------
        // Convert Classification code to a Name.
        // ------------------------------------------------------------
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = entities.LegalAction1.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
          export.LegalAction.Update.Classification.Text11 =
            local.CodeValue.Description;
        }
        else
        {
          export.LegalAction.Update.Classification.Text11 = "<INVALID>";
        }

        // ------------------------------------------------------------
        // Get the last date served.
        // ------------------------------------------------------------
        if (ReadServiceProcess())
        {
          export.LegalAction.Update.ServiceProcess.ServiceDate =
            entities.ServiceProcess.ServiceDate;
        }

        if (AsChar(local.TribPopulatedIndicator.Flag) == 'N')
        {
          local.TribPopulatedIndicator.Flag = "Y";
          export.SearchTribunal.Assign(entities.Tribunal);
          export.SearchFips.Assign(entities.Fips);
        }
      }
    }
    else
    {
      foreach(var item in ReadLegalActionTribunal2())
      {
        if (Equal(import.SearchLegalAction.CreatedTstamp,
          local.Null1.CreatedTstamp))
        {
          // *** Continue on need all legal actions
        }
        else if (Lt(entities.LegalAction1.CreatedTstamp,
          import.SearchLegalAction.CreatedTstamp))
        {
          // *** Continue on need all the legal action less than the import 
          // seach timestamp
        }
        else
        {
          continue;
        }

        // ------------------------------------------------------------
        // If the Classification has been entered, display only those
        // Legal Actions with that Classification for the specified
        // Court Case Number. If the Classification has not been
        // entered, display Legal Actions with any Classification for
        // the specified Court Case Number.
        // ------------------------------------------------------------
        if (!IsEmpty(import.SearchLegalAction.Classification))
        {
          if (AsChar(entities.LegalAction1.Classification) == AsChar
            (import.SearchLegalAction.Classification))
          {
            if (export.LegalAction.Index + 1 >= Export
              .LegalActionGroup.Capacity)
            {
              return;
            }

            ++export.LegalAction.Index;
            export.LegalAction.CheckSize();

            export.LegalAction.Update.LegalAction1.
              Assign(entities.LegalAction1);
          }
          else
          {
            continue;
          }
        }
        else
        {
          if (export.LegalAction.Index + 1 >= Export.LegalActionGroup.Capacity)
          {
            return;
          }

          ++export.LegalAction.Index;
          export.LegalAction.CheckSize();

          export.LegalAction.Update.LegalAction1.Assign(entities.LegalAction1);
        }

        if (ReadLegalActionAppeal())
        {
          export.LegalAction.Update.LappInd.Flag = "Y";
        }
        else
        {
          export.LegalAction.Update.LappInd.Flag = "";
        }

        UseLeGetActionTakenDescription();

        // ------------------------------------------------------------
        // Convert Classification code to a Name.
        // ------------------------------------------------------------
        local.Code.CodeName = "LEGAL ACTION CLASSIFICATION";
        local.CodeValue.Cdvalue = entities.LegalAction1.Classification;
        UseCabValidateCodeValue();

        if (AsChar(local.ValidCode.Flag) == 'Y')
        {
          export.LegalAction.Update.Classification.Text11 =
            local.CodeValue.Description;
        }
        else
        {
          export.LegalAction.Update.Classification.Text11 = "<INVALID>";
        }

        // ------------------------------------------------------------
        // Get the last date served.
        // ------------------------------------------------------------
        if (ReadServiceProcess())
        {
          export.LegalAction.Update.ServiceProcess.ServiceDate =
            entities.ServiceProcess.ServiceDate;
        }
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Code.CodeName;
    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, local.CodeValue);
    local.ValidCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseLeGetActionTakenDescription()
  {
    var useImport = new LeGetActionTakenDescription.Import();
    var useExport = new LeGetActionTakenDescription.Export();

    useImport.LegalAction.ActionTaken =
      export.LegalAction.Item.LegalAction1.ActionTaken;

    Call(LeGetActionTakenDescription.Execute, useImport, useExport);

    export.LegalAction.Update.LaActionTaken.Description =
      useExport.CodeValue.Description;
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.Tribunal.Populated);
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.Tribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county", entities.Tribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state", entities.Tribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.CountyDescription = db.GetNullableString(reader, 3);
        entities.Fips.StateAbbreviation = db.GetString(reader, 4);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 5);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadLegalActionAppeal()
  {
    entities.LegalActionAppeal.Populated = false;

    return Read("ReadLegalActionAppeal",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction1.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionAppeal.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionAppeal.AplId = db.GetInt32(reader, 1);
        entities.LegalActionAppeal.LgaId = db.GetInt32(reader, 2);
        entities.LegalActionAppeal.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal1()
  {
    entities.Tribunal.Populated = false;
    entities.LegalAction1.Populated = false;

    return ReadEach("ReadLegalActionTribunal1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.SearchLegalAction.CourtCaseNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.LegalAction1.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction1.Classification = db.GetString(reader, 1);
        entities.LegalAction1.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction1.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction1.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction1.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction1.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction1.ForeignOrderNumber =
          db.GetNullableString(reader, 7);
        entities.LegalAction1.TrbId = db.GetNullableInt32(reader, 8);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.LegalAction1.SystemGenInd = db.GetNullableString(reader, 9);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 10);
        entities.Tribunal.Name = db.GetString(reader, 11);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 12);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 13);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 14);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 15);
        entities.Tribunal.Populated = true;
        entities.LegalAction1.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionTribunal2()
  {
    entities.Tribunal.Populated = false;
    entities.LegalAction1.Populated = false;

    return ReadEach("ReadLegalActionTribunal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", import.SearchLegalAction.StandardNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.LegalAction1.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction1.Classification = db.GetString(reader, 1);
        entities.LegalAction1.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction1.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction1.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction1.StandardNumber = db.GetNullableString(reader, 5);
        entities.LegalAction1.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalAction1.ForeignOrderNumber =
          db.GetNullableString(reader, 7);
        entities.LegalAction1.TrbId = db.GetNullableInt32(reader, 8);
        entities.Tribunal.Identifier = db.GetInt32(reader, 8);
        entities.LegalAction1.SystemGenInd = db.GetNullableString(reader, 9);
        entities.Tribunal.JudicialDivision = db.GetNullableString(reader, 10);
        entities.Tribunal.Name = db.GetString(reader, 11);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 12);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 13);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 14);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 15);
        entities.Tribunal.Populated = true;
        entities.LegalAction1.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProcess()
  {
    entities.ServiceProcess.Populated = false;

    return Read("ReadServiceProcess",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction1.Identifier);
        db.SetDate(
          command, "serviceDate",
          local.InitialisedToZeros.ServiceDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 1);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 2);
        entities.ServiceProcess.Populated = true;
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
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    private Fips searchFips;
    private Tribunal searchTribunal;
    private LegalAction searchLegalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LegalActionGroup group.</summary>
    [Serializable]
    public class LegalActionGroup
    {
      /// <summary>
      /// A value of LaActionTaken.
      /// </summary>
      [JsonPropertyName("laActionTaken")]
      public CodeValue LaActionTaken
      {
        get => laActionTaken ??= new();
        set => laActionTaken = value;
      }

      /// <summary>
      /// A value of LappInd.
      /// </summary>
      [JsonPropertyName("lappInd")]
      public Common LappInd
      {
        get => lappInd ??= new();
        set => lappInd = value;
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

      /// <summary>
      /// A value of Classification.
      /// </summary>
      [JsonPropertyName("classification")]
      public WorkArea Classification
      {
        get => classification ??= new();
        set => classification = value;
      }

      /// <summary>
      /// A value of LegalAction1.
      /// </summary>
      [JsonPropertyName("legalAction1")]
      public LegalAction LegalAction1
      {
        get => legalAction1 ??= new();
        set => legalAction1 = value;
      }

      /// <summary>
      /// A value of ServiceProcess.
      /// </summary>
      [JsonPropertyName("serviceProcess")]
      public ServiceProcess ServiceProcess
      {
        get => serviceProcess ??= new();
        set => serviceProcess = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CodeValue laActionTaken;
      private Common lappInd;
      private Common common;
      private WorkArea classification;
      private LegalAction legalAction1;
      private ServiceProcess serviceProcess;
    }

    /// <summary>
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    /// <summary>
    /// Gets a value of LegalAction.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionGroup> LegalAction => legalAction ??= new(
      LegalActionGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalAction for json serialization.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Computed]
    public IList<LegalActionGroup> LegalAction_Json
    {
      get => legalAction;
      set => LegalAction.Assign(value);
    }

    /// <summary>
    /// A value of SearchTribunal.
    /// </summary>
    [JsonPropertyName("searchTribunal")]
    public Tribunal SearchTribunal
    {
      get => searchTribunal ??= new();
      set => searchTribunal = value;
    }

    /// <summary>
    /// A value of SearchFips.
    /// </summary>
    [JsonPropertyName("searchFips")]
    public Fips SearchFips
    {
      get => searchFips ??= new();
      set => searchFips = value;
    }

    private LegalAction searchLegalAction;
    private Array<LegalActionGroup> legalAction;
    private Tribunal searchTribunal;
    private Fips searchFips;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public ServiceProcess InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    /// <summary>
    /// A value of TribPopulatedIndicator.
    /// </summary>
    [JsonPropertyName("tribPopulatedIndicator")]
    public Common TribPopulatedIndicator
    {
      get => tribPopulatedIndicator ??= new();
      set => tribPopulatedIndicator = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public LegalAction Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

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

    /// <summary>
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    private ServiceProcess initialisedToZeros;
    private Common tribPopulatedIndicator;
    private LegalAction null1;
    private Code code;
    private CodeValue codeValue;
    private Common validCode;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LegalActionAppeal.
    /// </summary>
    [JsonPropertyName("legalActionAppeal")]
    public LegalActionAppeal LegalActionAppeal
    {
      get => legalActionAppeal ??= new();
      set => legalActionAppeal = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalAction1.
    /// </summary>
    [JsonPropertyName("legalAction1")]
    public LegalAction LegalAction1
    {
      get => legalAction1 ??= new();
      set => legalAction1 = value;
    }

    /// <summary>
    /// A value of Initial.
    /// </summary>
    [JsonPropertyName("initial")]
    public LegalAction Initial
    {
      get => initial ??= new();
      set => initial = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    private LegalActionAppeal legalActionAppeal;
    private Tribunal tribunal;
    private Fips fips;
    private LegalAction legalAction1;
    private LegalAction initial;
    private ServiceProcess serviceProcess;
  }
#endregion
}
