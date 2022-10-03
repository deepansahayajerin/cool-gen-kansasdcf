// Program: SI_INRD_DELETE_INFORMATION_REQ, ID: 371426556, model: 746.
// Short name: SWE01159
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_INRD_DELETE_INFORMATION_REQ.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiInrdDeleteInformationReq: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_INRD_DELETE_INFORMATION_REQ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiInrdDeleteInformationReq(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiInrdDeleteInformationReq.
  /// </summary>
  public SiInrdDeleteInformationReq(IContext context, Import import,
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
    // ---------------------------------------------
    //        M A I N T E N A N C E   L O G
    //  Date   Developer    Description
    // 7-10-95 Ken Evans    Initial Development
    // ---------------------------------------------
    // 07/06/99 W.Campbell  Modified the properties
    //                      of three READ statement to
    //                      Select Only.
    // -----------------------------------------------
    // 08/07/00 W.Campbell  Added logic to this
    //                      CAB for the new attribute:
    //                      Application Processed IND
    //                      which was added to entity
    //                      type information_request.
    //                      Work done on PR# 100532.
    // ------------------------------------------------------------
    // -----------------------------------------------
    // 07/06/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // -----------------------------------------------
    if (ReadInformationRequest())
    {
      // -----------------------------------------------
      // 07/06/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // -----------------------------------------------
      if (ReadCase2())
      {
        ExitState = "SI0000_CANT_DELETE_INRD";
      }
      else
      {
        // -----------------------------------------------
        // 07/06/99 W.Campbell - Modified the properties
        // of the following READ statement to Select Only.
        // -----------------------------------------------
        if (ReadCase1())
        {
          ExitState = "SI0000_CANT_DELETE_INRD";
        }
        else
        {
          // ------------------------------------------------------------
          // 08/07/00 W.Campbell - Added logic to this
          // CAB for the new attribute:
          // Application Processed IND
          // which was added to entity
          // type information_request.
          // Work done on PR# 100532.
          // ------------------------------------------------------------
          // ------------------------------------------------------------
          // 08/07/00 W.Campbell - Do not allow deletion
          // of Info Request if Application Processed IND
          // is equal to 'Y'.  This indicates that this INFO
          // REQ was used in working a CSE case.
          // Work done on PR# 100532.
          // ------------------------------------------------------------
          if (AsChar(entities.InformationRequest.ApplicationProcessedInd) == 'Y'
            )
          {
            ExitState = "SI0000_CANT_DELETE_INFO_REQ";

            return;
          }

          DeleteInformationRequest();
        }
      }
    }
    else
    {
      ExitState = "INQUIRY_NF";
    }
  }

  private void DeleteInformationRequest()
  {
    Update("DeleteInformationRequest#1",
      (db, command) =>
      {
        db.SetInt64(command, "inqNo", entities.InformationRequest.Number);
      });

    Update("DeleteInformationRequest#2",
      (db, command) =>
      {
        db.SetInt64(command, "inqNo", entities.InformationRequest.Number);
      });
  }

  private bool ReadCase1()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetNullableInt64(
          command, "infoRequestNo", import.InformationRequest.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    System.Diagnostics.Debug.Assert(entities.InformationRequest.Populated);
    entities.Case1.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.
          SetString(command, "numb", entities.InformationRequest.FkCktCasenumb);
          
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 1);
        entities.Case1.Populated = true;
      });
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
        entities.InformationRequest.ApplicationProcessedInd =
          db.GetNullableString(reader, 1);
        entities.InformationRequest.FkCktCasenumb = db.GetString(reader, 2);
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
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private Case1 case1;
    private InformationRequest informationRequest;
  }
#endregion
}
