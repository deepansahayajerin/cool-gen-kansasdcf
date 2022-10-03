// Program: SI_CAB_DELETE_IS_CONTACT_ADDRESS, ID: 373465510, model: 746.
// Short name: SWE02796
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_DELETE_IS_CONTACT_ADDRESS.
/// </summary>
[Serializable]
public partial class SiCabDeleteIsContactAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_DELETE_IS_CONTACT_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabDeleteIsContactAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabDeleteIsContactAddress.
  /// </summary>
  public SiCabDeleteIsContactAddress(IContext context, Import import,
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
    if (import.InterstateRequest.IntHGeneratedId == 0)
    {
      ExitState = "INTERSTATE_REQUEST_NF";

      return;
    }

    if (Equal(import.InterstateContact.StartDate, local.Null1.Date))
    {
      foreach(var item in ReadInterstateContactAddress3())
      {
        DeleteInterstateContactAddress();
      }
    }
    else if (Equal(import.InterstateContactAddress.StartDate, local.Null1.Date))
    {
      foreach(var item in ReadInterstateContactAddress2())
      {
        DeleteInterstateContactAddress();
      }
    }
    else if (ReadInterstateContactAddress1())
    {
      DeleteInterstateContactAddress();
    }
  }

  private void DeleteInterstateContactAddress()
  {
    Update("DeleteInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          entities.InterstateContactAddress.IcoContStartDt.GetValueOrDefault());
          
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateContactAddress.IntGeneratedId);
        db.SetDate(
          command, "startDate",
          entities.InterstateContactAddress.StartDate.GetValueOrDefault());
      });
  }

  private bool ReadInterstateContactAddress1()
  {
    entities.InterstateContactAddress.Populated = false;

    return Read("ReadInterstateContactAddress1",
      (db, command) =>
      {
        db.SetDate(
          command, "startDate",
          import.InterstateContactAddress.StartDate.GetValueOrDefault());
        db.SetDate(
          command, "icoContStartDt",
          import.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateContactAddress2()
  {
    entities.InterstateContactAddress.Populated = false;

    return ReadEach("ReadInterstateContactAddress2",
      (db, command) =>
      {
        db.SetDate(
          command, "icoContStartDt",
          import.InterstateContact.StartDate.GetValueOrDefault());
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateContactAddress3()
  {
    entities.InterstateContactAddress.Populated = false;

    return ReadEach("ReadInterstateContactAddress3",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContactAddress.IcoContStartDt =
          db.GetDate(reader, 0);
        entities.InterstateContactAddress.IntGeneratedId =
          db.GetInt32(reader, 1);
        entities.InterstateContactAddress.StartDate = db.GetDate(reader, 2);
        entities.InterstateContactAddress.Populated = true;

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
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    private InterstateRequest interstateRequest;
    private InterstateContact interstateContact;
    private InterstateContactAddress interstateContactAddress;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateContactAddress interstateContactAddress;
    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
  }
#endregion
}
