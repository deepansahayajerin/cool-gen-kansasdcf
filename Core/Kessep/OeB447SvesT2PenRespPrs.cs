// Program: OE_B447_SVES_T2PEN_RESP_PRS, ID: 945066135, model: 746.
// Short name: SWE04472
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_B447_SVES_T2PEN_RESP_PRS.
/// </para>
/// <para>
/// This Action Block maintains SVES Title II pending information received from 
/// FCR.  I.e. if the SVES T2 response is not available for the selected CSE
/// person then AB will add a new record otherwise updates the existing records
/// the new information received from FCR.
/// </para>
/// </summary>
[Serializable]
public partial class OeB447SvesT2PenRespPrs: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B447_SVES_T2PEN_RESP_PRS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB447SvesT2PenRespPrs(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB447SvesT2PenRespPrs.
  /// </summary>
  public OeB447SvesT2PenRespPrs(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ******************************************************************************************
    // * This Action Block received the Title-II Pending information from the 
    // calling object    *
    // * and process them by adding/upding to CSE database and create required 
    // worker alerts    *
    // * income source & document 
    // generation wherever required.
    // 
    // *
    // ******************************************************************************************
    // ******************************************************************************************
    // *                                  
    // Maintenance Log
    // 
    // *
    // ******************************************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // *
    // * ----------  -----------------  ---------   
    // --------------------------------------------*
    // * 06/03/2011  Raj S              CQ5577      Initial Coding.
    // *
    // *
    // 
    // *
    // ******************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    local.Infrastructure.Assign(import.Infrastructure);
    local.Process.Date = local.Infrastructure.ReferenceDate;

    // *******************************************************************************************
    // ** Check whether receved Titl-II Pending claim record exists in CSE 
    // database then update **
    // ** the existing information otherwise create a new Title-II pending claim
    // entry to CSE   **
    // ** database.
    // 
    // **
    // *******************************************************************************************
    if (ReadFcrSvesGenInfo())
    {
      if (ReadFcrSvesTitleIiPend())
      {
        try
        {
          UpdateFcrSvesTitleIiPend();
          ++import.TotT2PendUpdated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_TITLEII_PEND_NU";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_TITLEII_PEND_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        try
        {
          CreateFcrSvesTitleIiPend();
          ++import.TotT2PendCreated.Count;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FCR_SVES_TITLEII_PEND_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FCR_SVES_TITLEII_PEND_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      ExitState = "FCR_SVES_GEN_INFO_NF";

      return;
    }

    // ******************************************************************************************
    // * Geenrate alerts, if the person plays a role AP or CH in any of CSE 
    // Cases, the person   *
    // * should be active as well as the case.
    // 
    // *
    // ******************************************************************************************
    UseOeB447SvesAlertNIwoGen();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      import.TotT2PendAlertCreated.Count += local.TotAlertRecsCreated.Count;
      import.TotT2PendAlertExists.Count += local.TotAlertExistsRecs.Count;
      import.TotT2PendHistCreated.Count += local.TotHistRecsCreated.Count;
      import.TotT2PendHistExists.Count += local.TotHistExistsRecs.Count;
      import.TotT2PendArlettCreated.Count += local.TotArLetterRecs.Count;
      import.TotT2PendIwoGenerated.Count += local.TotIwoRecs.Count;
    }
    else
    {
      ExitState = "ACO_NN0000_ALL_OK";
    }
  }

  private static void MoveFcrSvesGenInfo(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.MemberId = source.MemberId;
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
  }

  private void UseOeB447SvesAlertNIwoGen()
  {
    var useImport = new OeB447SvesAlertNIwoGen.Import();
    var useExport = new OeB447SvesAlertNIwoGen.Export();

    useImport.IwoGenerationSkipFl.Flag = import.IwoGenerationSkipFl.Flag;
    useImport.AlertGenerationSkipFl.Flag = import.AlertGenerationSkipFl.Flag;
    useImport.Max.Date = import.MaxDate.Date;
    MoveFcrSvesGenInfo(import.FcrSvesGenInfo, useImport.FcrSvesGenInfo);
    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.ProcessingDate.Date = local.Process.Date;

    Call(OeB447SvesAlertNIwoGen.Execute, useImport, useExport);

    local.TotAlertRecsCreated.Count = useExport.TotAlertRecsCreated.Count;
    local.TotHistRecsCreated.Count = useExport.TotHistRecsCreated.Count;
    local.TotAlertExistsRecs.Count = useExport.TotAlertExistsRecs.Count;
    local.TotHistExistsRecs.Count = useExport.TotHistExistsRecs.Count;
    local.TotArLetterRecs.Count = useExport.TotArLetterRecs.Count;
    local.TotIwoRecs.Count = useExport.TotIwoRecs.Count;
  }

  private void CreateFcrSvesTitleIiPend()
  {
    var fcgMemberId = entities.ExistingFcrSvesGenInfo.MemberId;
    var fcgLSRspAgy =
      entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo;
    var seqNo = import.FcrSvesTitleIiPend.SeqNo;
    var nameMatchedCode = import.FcrSvesTitleIiPend.NameMatchedCode ?? "";
    var responseDate = import.FcrSvesTitleIiPend.ResponseDate;
    var otherSsn = import.FcrSvesTitleIiPend.OtherSsn ?? "";
    var ssnMatchCode = import.FcrSvesTitleIiPend.SsnMatchCode ?? "";
    var claimTypeCode = import.FcrSvesTitleIiPend.ClaimTypeCode ?? "";
    var createdBy = import.FcrSvesTitleIiPend.CreatedBy;
    var createdTimestamp = import.FcrSvesTitleIiPend.CreatedTimestamp;
    var lastUpdatedBy = local.Null1.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.Null1.LastUpdatedTimestamp;
    var firstNameText = import.FcrSvesTitleIiPend.FirstNameText ?? "";
    var middleNameText = import.FcrSvesTitleIiPend.MiddleNameText ?? "";
    var lastNameText = import.FcrSvesTitleIiPend.LastNameText ?? "";
    var additionalFirstName1Text =
      import.FcrSvesTitleIiPend.AdditionalFirstName1Text ?? "";
    var additionalMiddleName1Text =
      import.FcrSvesTitleIiPend.AdditionalMiddleName1Text ?? "";
    var additionalLastName1Text =
      import.FcrSvesTitleIiPend.AdditionalLastName1Text ?? "";
    var additionalFirstName2Text =
      import.FcrSvesTitleIiPend.AdditionalFirstName2Text ?? "";
    var additionalMiddleName2Text =
      import.FcrSvesTitleIiPend.AdditionalMiddleName2Text ?? "";
    var additionalLastName2Text =
      import.FcrSvesTitleIiPend.AdditionalLastName2Text ?? "";

    entities.ExistingFcrSvesTitleIiPend.Populated = false;
    Update("CreateFcrSvesTitleIiPend",
      (db, command) =>
      {
        db.SetString(command, "fcgMemberId", fcgMemberId);
        db.SetString(command, "fcgLSRspAgy", fcgLSRspAgy);
        db.SetInt32(command, "seqNo", seqNo);
        db.SetNullableString(command, "nameMatchedCode", nameMatchedCode);
        db.SetNullableDate(command, "responseDate", responseDate);
        db.SetNullableString(command, "otherSsn", otherSsn);
        db.SetNullableString(command, "ssnMatchCode", ssnMatchCode);
        db.SetNullableString(command, "claimTypeCode", claimTypeCode);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "fcrFirstName", firstNameText);
        db.SetNullableString(command, "fcrMiddleName", middleNameText);
        db.SetNullableString(command, "fcrLastName", lastNameText);
        db.
          SetNullableString(command, "ad1StName1Txt", additionalFirstName1Text);
          
        db.
          SetNullableString(command, "adMidName1Txt", additionalMiddleName1Text);
          
        db.SetNullableString(command, "adLstName1Txt", additionalLastName1Text);
        db.
          SetNullableString(command, "ad1StName2Txt", additionalFirstName2Text);
          
        db.
          SetNullableString(command, "adMidName2Txt", additionalMiddleName2Text);
          
        db.SetNullableString(command, "adLstName2Txt", additionalLastName2Text);
      });

    entities.ExistingFcrSvesTitleIiPend.FcgMemberId = fcgMemberId;
    entities.ExistingFcrSvesTitleIiPend.FcgLSRspAgy = fcgLSRspAgy;
    entities.ExistingFcrSvesTitleIiPend.SeqNo = seqNo;
    entities.ExistingFcrSvesTitleIiPend.NameMatchedCode = nameMatchedCode;
    entities.ExistingFcrSvesTitleIiPend.ResponseDate = responseDate;
    entities.ExistingFcrSvesTitleIiPend.OtherSsn = otherSsn;
    entities.ExistingFcrSvesTitleIiPend.SsnMatchCode = ssnMatchCode;
    entities.ExistingFcrSvesTitleIiPend.ClaimTypeCode = claimTypeCode;
    entities.ExistingFcrSvesTitleIiPend.CreatedBy = createdBy;
    entities.ExistingFcrSvesTitleIiPend.CreatedTimestamp = createdTimestamp;
    entities.ExistingFcrSvesTitleIiPend.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesTitleIiPend.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFcrSvesTitleIiPend.FirstNameText = firstNameText;
    entities.ExistingFcrSvesTitleIiPend.MiddleNameText = middleNameText;
    entities.ExistingFcrSvesTitleIiPend.LastNameText = lastNameText;
    entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName1Text =
      additionalFirstName1Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName1Text =
      additionalMiddleName1Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalLastName1Text =
      additionalLastName1Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName2Text =
      additionalFirstName2Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName2Text =
      additionalMiddleName2Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalLastName2Text =
      additionalLastName2Text;
    entities.ExistingFcrSvesTitleIiPend.Populated = true;
  }

  private bool ReadFcrSvesGenInfo()
  {
    entities.ExistingFcrSvesGenInfo.Populated = false;

    return Read("ReadFcrSvesGenInfo",
      (db, command) =>
      {
        db.SetString(command, "memberId", import.FcrSvesGenInfo.MemberId);
        db.SetString(
          command, "locSrcRspAgyCd",
          import.FcrSvesGenInfo.LocateSourceResponseAgencyCo);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesGenInfo.MemberId = db.GetString(reader, 0);
        entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesGenInfo.Populated = true;
      });
  }

  private bool ReadFcrSvesTitleIiPend()
  {
    entities.ExistingFcrSvesTitleIiPend.Populated = false;

    return Read("ReadFcrSvesTitleIiPend",
      (db, command) =>
      {
        db.SetInt32(command, "seqNo", import.FcrSvesTitleIiPend.SeqNo);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesGenInfo.LocateSourceResponseAgencyCo);
        db.SetString(
          command, "fcgMemberId", entities.ExistingFcrSvesGenInfo.MemberId);
      },
      (db, reader) =>
      {
        entities.ExistingFcrSvesTitleIiPend.FcgMemberId =
          db.GetString(reader, 0);
        entities.ExistingFcrSvesTitleIiPend.FcgLSRspAgy =
          db.GetString(reader, 1);
        entities.ExistingFcrSvesTitleIiPend.SeqNo = db.GetInt32(reader, 2);
        entities.ExistingFcrSvesTitleIiPend.NameMatchedCode =
          db.GetNullableString(reader, 3);
        entities.ExistingFcrSvesTitleIiPend.ResponseDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingFcrSvesTitleIiPend.OtherSsn =
          db.GetNullableString(reader, 5);
        entities.ExistingFcrSvesTitleIiPend.SsnMatchCode =
          db.GetNullableString(reader, 6);
        entities.ExistingFcrSvesTitleIiPend.ClaimTypeCode =
          db.GetNullableString(reader, 7);
        entities.ExistingFcrSvesTitleIiPend.CreatedBy = db.GetString(reader, 8);
        entities.ExistingFcrSvesTitleIiPend.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingFcrSvesTitleIiPend.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.ExistingFcrSvesTitleIiPend.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingFcrSvesTitleIiPend.FirstNameText =
          db.GetNullableString(reader, 12);
        entities.ExistingFcrSvesTitleIiPend.MiddleNameText =
          db.GetNullableString(reader, 13);
        entities.ExistingFcrSvesTitleIiPend.LastNameText =
          db.GetNullableString(reader, 14);
        entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName1Text =
          db.GetNullableString(reader, 15);
        entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName1Text =
          db.GetNullableString(reader, 16);
        entities.ExistingFcrSvesTitleIiPend.AdditionalLastName1Text =
          db.GetNullableString(reader, 17);
        entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName2Text =
          db.GetNullableString(reader, 18);
        entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName2Text =
          db.GetNullableString(reader, 19);
        entities.ExistingFcrSvesTitleIiPend.AdditionalLastName2Text =
          db.GetNullableString(reader, 20);
        entities.ExistingFcrSvesTitleIiPend.Populated = true;
      });
  }

  private void UpdateFcrSvesTitleIiPend()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFcrSvesTitleIiPend.Populated);

    var nameMatchedCode = import.FcrSvesTitleIiPend.NameMatchedCode ?? "";
    var responseDate = import.FcrSvesTitleIiPend.ResponseDate;
    var otherSsn = import.FcrSvesTitleIiPend.OtherSsn ?? "";
    var ssnMatchCode = import.FcrSvesTitleIiPend.SsnMatchCode ?? "";
    var claimTypeCode = import.FcrSvesTitleIiPend.ClaimTypeCode ?? "";
    var lastUpdatedBy = import.FcrSvesTitleIiPend.CreatedBy;
    var lastUpdatedTimestamp = import.FcrSvesTitleIiPend.CreatedTimestamp;
    var firstNameText = import.FcrSvesTitleIiPend.FirstNameText ?? "";
    var middleNameText = import.FcrSvesTitleIiPend.MiddleNameText ?? "";
    var lastNameText = import.FcrSvesTitleIiPend.LastNameText ?? "";
    var additionalFirstName1Text =
      import.FcrSvesTitleIiPend.AdditionalFirstName1Text ?? "";
    var additionalMiddleName1Text =
      import.FcrSvesTitleIiPend.AdditionalMiddleName1Text ?? "";
    var additionalLastName1Text =
      import.FcrSvesTitleIiPend.AdditionalLastName1Text ?? "";
    var additionalFirstName2Text =
      import.FcrSvesTitleIiPend.AdditionalFirstName2Text ?? "";
    var additionalMiddleName2Text =
      import.FcrSvesTitleIiPend.AdditionalMiddleName2Text ?? "";
    var additionalLastName2Text =
      import.FcrSvesTitleIiPend.AdditionalLastName2Text ?? "";

    entities.ExistingFcrSvesTitleIiPend.Populated = false;
    Update("UpdateFcrSvesTitleIiPend",
      (db, command) =>
      {
        db.SetNullableString(command, "nameMatchedCode", nameMatchedCode);
        db.SetNullableDate(command, "responseDate", responseDate);
        db.SetNullableString(command, "otherSsn", otherSsn);
        db.SetNullableString(command, "ssnMatchCode", ssnMatchCode);
        db.SetNullableString(command, "claimTypeCode", claimTypeCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "fcrFirstName", firstNameText);
        db.SetNullableString(command, "fcrMiddleName", middleNameText);
        db.SetNullableString(command, "fcrLastName", lastNameText);
        db.
          SetNullableString(command, "ad1StName1Txt", additionalFirstName1Text);
          
        db.
          SetNullableString(command, "adMidName1Txt", additionalMiddleName1Text);
          
        db.SetNullableString(command, "adLstName1Txt", additionalLastName1Text);
        db.
          SetNullableString(command, "ad1StName2Txt", additionalFirstName2Text);
          
        db.
          SetNullableString(command, "adMidName2Txt", additionalMiddleName2Text);
          
        db.SetNullableString(command, "adLstName2Txt", additionalLastName2Text);
        db.SetString(
          command, "fcgMemberId",
          entities.ExistingFcrSvesTitleIiPend.FcgMemberId);
        db.SetString(
          command, "fcgLSRspAgy",
          entities.ExistingFcrSvesTitleIiPend.FcgLSRspAgy);
        db.
          SetInt32(command, "seqNo", entities.ExistingFcrSvesTitleIiPend.SeqNo);
          
      });

    entities.ExistingFcrSvesTitleIiPend.NameMatchedCode = nameMatchedCode;
    entities.ExistingFcrSvesTitleIiPend.ResponseDate = responseDate;
    entities.ExistingFcrSvesTitleIiPend.OtherSsn = otherSsn;
    entities.ExistingFcrSvesTitleIiPend.SsnMatchCode = ssnMatchCode;
    entities.ExistingFcrSvesTitleIiPend.ClaimTypeCode = claimTypeCode;
    entities.ExistingFcrSvesTitleIiPend.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingFcrSvesTitleIiPend.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ExistingFcrSvesTitleIiPend.FirstNameText = firstNameText;
    entities.ExistingFcrSvesTitleIiPend.MiddleNameText = middleNameText;
    entities.ExistingFcrSvesTitleIiPend.LastNameText = lastNameText;
    entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName1Text =
      additionalFirstName1Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName1Text =
      additionalMiddleName1Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalLastName1Text =
      additionalLastName1Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalFirstName2Text =
      additionalFirstName2Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalMiddleName2Text =
      additionalMiddleName2Text;
    entities.ExistingFcrSvesTitleIiPend.AdditionalLastName2Text =
      additionalLastName2Text;
    entities.ExistingFcrSvesTitleIiPend.Populated = true;
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
    /// A value of AlertGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("alertGenerationSkipFl")]
    public Common AlertGenerationSkipFl
    {
      get => alertGenerationSkipFl ??= new();
      set => alertGenerationSkipFl = value;
    }

    /// <summary>
    /// A value of IwoGenerationSkipFl.
    /// </summary>
    [JsonPropertyName("iwoGenerationSkipFl")]
    public Common IwoGenerationSkipFl
    {
      get => iwoGenerationSkipFl ??= new();
      set => iwoGenerationSkipFl = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleIiPend.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleIiPend")]
    public FcrSvesTitleIiPend FcrSvesTitleIiPend
    {
      get => fcrSvesTitleIiPend ??= new();
      set => fcrSvesTitleIiPend = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of TotT2PendAlertExists.
    /// </summary>
    [JsonPropertyName("totT2PendAlertExists")]
    public Common TotT2PendAlertExists
    {
      get => totT2PendAlertExists ??= new();
      set => totT2PendAlertExists = value;
    }

    /// <summary>
    /// A value of TotT2PendHistExists.
    /// </summary>
    [JsonPropertyName("totT2PendHistExists")]
    public Common TotT2PendHistExists
    {
      get => totT2PendHistExists ??= new();
      set => totT2PendHistExists = value;
    }

    /// <summary>
    /// A value of TotT2PendArlettCreated.
    /// </summary>
    [JsonPropertyName("totT2PendArlettCreated")]
    public Common TotT2PendArlettCreated
    {
      get => totT2PendArlettCreated ??= new();
      set => totT2PendArlettCreated = value;
    }

    /// <summary>
    /// A value of TotT2PendIwoGenerated.
    /// </summary>
    [JsonPropertyName("totT2PendIwoGenerated")]
    public Common TotT2PendIwoGenerated
    {
      get => totT2PendIwoGenerated ??= new();
      set => totT2PendIwoGenerated = value;
    }

    /// <summary>
    /// A value of TotT2PendAlertCreated.
    /// </summary>
    [JsonPropertyName("totT2PendAlertCreated")]
    public Common TotT2PendAlertCreated
    {
      get => totT2PendAlertCreated ??= new();
      set => totT2PendAlertCreated = value;
    }

    /// <summary>
    /// A value of TotT2PendHistCreated.
    /// </summary>
    [JsonPropertyName("totT2PendHistCreated")]
    public Common TotT2PendHistCreated
    {
      get => totT2PendHistCreated ??= new();
      set => totT2PendHistCreated = value;
    }

    /// <summary>
    /// A value of TotT2PendUpdated.
    /// </summary>
    [JsonPropertyName("totT2PendUpdated")]
    public Common TotT2PendUpdated
    {
      get => totT2PendUpdated ??= new();
      set => totT2PendUpdated = value;
    }

    /// <summary>
    /// A value of TotT2PendCreated.
    /// </summary>
    [JsonPropertyName("totT2PendCreated")]
    public Common TotT2PendCreated
    {
      get => totT2PendCreated ??= new();
      set => totT2PendCreated = value;
    }

    private Common alertGenerationSkipFl;
    private Common iwoGenerationSkipFl;
    private DateWorkArea maxDate;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesTitleIiPend fcrSvesTitleIiPend;
    private Infrastructure infrastructure;
    private Common totT2PendAlertExists;
    private Common totT2PendHistExists;
    private Common totT2PendArlettCreated;
    private Common totT2PendIwoGenerated;
    private Common totT2PendAlertCreated;
    private Common totT2PendHistCreated;
    private Common totT2PendUpdated;
    private Common totT2PendCreated;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotIwoRecs.
    /// </summary>
    [JsonPropertyName("totIwoRecs")]
    public Common TotIwoRecs
    {
      get => totIwoRecs ??= new();
      set => totIwoRecs = value;
    }

    /// <summary>
    /// A value of TotArLetterRecs.
    /// </summary>
    [JsonPropertyName("totArLetterRecs")]
    public Common TotArLetterRecs
    {
      get => totArLetterRecs ??= new();
      set => totArLetterRecs = value;
    }

    /// <summary>
    /// A value of TotHistExistsRecs.
    /// </summary>
    [JsonPropertyName("totHistExistsRecs")]
    public Common TotHistExistsRecs
    {
      get => totHistExistsRecs ??= new();
      set => totHistExistsRecs = value;
    }

    /// <summary>
    /// A value of TotAlertExistsRecs.
    /// </summary>
    [JsonPropertyName("totAlertExistsRecs")]
    public Common TotAlertExistsRecs
    {
      get => totAlertExistsRecs ??= new();
      set => totAlertExistsRecs = value;
    }

    /// <summary>
    /// A value of TotHistRecsCreated.
    /// </summary>
    [JsonPropertyName("totHistRecsCreated")]
    public Common TotHistRecsCreated
    {
      get => totHistRecsCreated ??= new();
      set => totHistRecsCreated = value;
    }

    /// <summary>
    /// A value of TotAlertRecsCreated.
    /// </summary>
    [JsonPropertyName("totAlertRecsCreated")]
    public Common TotAlertRecsCreated
    {
      get => totAlertRecsCreated ??= new();
      set => totAlertRecsCreated = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public FcrSvesTitleIiPend Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private Common totIwoRecs;
    private Common totArLetterRecs;
    private Common totHistExistsRecs;
    private Common totAlertExistsRecs;
    private Common totHistRecsCreated;
    private Common totAlertRecsCreated;
    private FcrSvesTitleIiPend null1;
    private Infrastructure infrastructure;
    private DateWorkArea process;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("existingFcrSvesGenInfo")]
    public FcrSvesGenInfo ExistingFcrSvesGenInfo
    {
      get => existingFcrSvesGenInfo ??= new();
      set => existingFcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of ExistingFcrSvesTitleIiPend.
    /// </summary>
    [JsonPropertyName("existingFcrSvesTitleIiPend")]
    public FcrSvesTitleIiPend ExistingFcrSvesTitleIiPend
    {
      get => existingFcrSvesTitleIiPend ??= new();
      set => existingFcrSvesTitleIiPend = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private FcrSvesGenInfo existingFcrSvesGenInfo;
    private FcrSvesTitleIiPend existingFcrSvesTitleIiPend;
    private CsePerson csePerson;
  }
#endregion
}
