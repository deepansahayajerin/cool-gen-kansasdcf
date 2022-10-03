// Program: SI_INRD_READ_INFORMATION_REQ, ID: 371426553, model: 746.
// Short name: SWE01225
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INRD_READ_INFORMATION_REQ.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiInrdReadInformationReq: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INRD_READ_INFORMATION_REQ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInrdReadInformationReq(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInrdReadInformationReq.
  /// </summary>
  public SiInrdReadInformationReq(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date   Developer    Description
    // 7-10-95 Ken Evans    Initial Development
    // ---------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 08/07/00 W.Campbell             Added new attribute
    //                                 
    // to Export and Entity view for
    // new
    //                                 
    // attribute Application Processed
    // IND
    //                                 
    // which had been added to
    //                                 
    // entity type information_request.
    //                                 
    // Work done on PR# 100532.
    // ------------------------------------------------------------
    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadInformationRequest())
    {
      export.InformationRequest.Assign(entities.InformationRequest);
    }
    else
    {
      ExitState = "INQUIRY_NF";
    }
  }

  private bool ReadInformationRequest()
  {
    entities.InformationRequest.Populated = false;

    return Read("ReadInformationRequest",
      (db, command) =>
      {
        db.SetInt64(command, "numb", import.InformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.InformationRequest.Number = db.GetInt64(reader, 0);
        entities.InformationRequest.NonparentQuestionnaireSent =
          db.GetNullableString(reader, 1);
        entities.InformationRequest.ParentQuestionnaireSent =
          db.GetNullableString(reader, 2);
        entities.InformationRequest.PaternityQuestionnaireSent =
          db.GetNullableString(reader, 3);
        entities.InformationRequest.ApplicationSentIndicator =
          db.GetString(reader, 4);
        entities.InformationRequest.QuestionnaireTypeIndicator =
          db.GetString(reader, 5);
        entities.InformationRequest.DateReceivedByCseComplete =
          db.GetNullableDate(reader, 6);
        entities.InformationRequest.DateReceivedByCseIncomplete =
          db.GetNullableDate(reader, 7);
        entities.InformationRequest.DateApplicationRequested =
          db.GetDate(reader, 8);
        entities.InformationRequest.CallerLastName =
          db.GetNullableString(reader, 9);
        entities.InformationRequest.CallerFirstName = db.GetString(reader, 10);
        entities.InformationRequest.CallerMiddleInitial =
          db.GetString(reader, 11);
        entities.InformationRequest.InquirerNameSuffix =
          db.GetNullableString(reader, 12);
        entities.InformationRequest.ApplicantLastName =
          db.GetNullableString(reader, 13);
        entities.InformationRequest.ApplicantFirstName =
          db.GetNullableString(reader, 14);
        entities.InformationRequest.ApplicantMiddleInitial =
          db.GetNullableString(reader, 15);
        entities.InformationRequest.ApplicantNameSuffix =
          db.GetString(reader, 16);
        entities.InformationRequest.ApplicantStreet1 =
          db.GetNullableString(reader, 17);
        entities.InformationRequest.ApplicantStreet2 =
          db.GetNullableString(reader, 18);
        entities.InformationRequest.ApplicantCity =
          db.GetNullableString(reader, 19);
        entities.InformationRequest.ApplicantState =
          db.GetNullableString(reader, 20);
        entities.InformationRequest.ApplicantZip5 =
          db.GetNullableString(reader, 21);
        entities.InformationRequest.ApplicantZip4 =
          db.GetNullableString(reader, 22);
        entities.InformationRequest.ApplicantZip3 =
          db.GetNullableString(reader, 23);
        entities.InformationRequest.ApplicantPhone =
          db.GetNullableInt32(reader, 24);
        entities.InformationRequest.DateApplicationSent =
          db.GetNullableDate(reader, 25);
        entities.InformationRequest.Type1 = db.GetString(reader, 26);
        entities.InformationRequest.ServiceCode =
          db.GetNullableString(reader, 27);
        entities.InformationRequest.ReasonIncomplete =
          db.GetNullableString(reader, 28);
        entities.InformationRequest.Note = db.GetNullableString(reader, 29);
        entities.InformationRequest.CreatedBy = db.GetString(reader, 30);
        entities.InformationRequest.CreatedTimestamp =
          db.GetDateTime(reader, 31);
        entities.InformationRequest.LastUpdatedBy =
          db.GetNullableString(reader, 32);
        entities.InformationRequest.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 33);
        entities.InformationRequest.ReasonDenied =
          db.GetNullableString(reader, 34);
        entities.InformationRequest.DateDenied = db.GetNullableDate(reader, 35);
        entities.InformationRequest.ApplicantAreaCode =
          db.GetNullableInt32(reader, 36);
        entities.InformationRequest.ApplicationProcessedInd =
          db.GetNullableString(reader, 37);
        entities.InformationRequest.NameSearchComplete =
          db.GetNullableString(reader, 38);
        entities.InformationRequest.ReopenReasonType =
          db.GetNullableString(reader, 39);
        entities.InformationRequest.MiscellaneousReason =
          db.GetNullableString(reader, 40);
        entities.InformationRequest.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
  }
#endregion
}
