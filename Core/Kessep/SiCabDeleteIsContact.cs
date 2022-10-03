// Program: SI_CAB_DELETE_IS_CONTACT, ID: 373465506, model: 746.
// Short name: SWE02795
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_DELETE_IS_CONTACT.
/// </summary>
[Serializable]
public partial class SiCabDeleteIsContact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_DELETE_IS_CONTACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabDeleteIsContact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabDeleteIsContact.
  /// </summary>
  public SiCabDeleteIsContact(IContext context, Import import, Export export):
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

    foreach(var item in ReadInterstateContact())
    {
      UseSiCabDeleteIsContactAddress();
      DeleteInterstateContact();
    }
  }

  private void UseSiCabDeleteIsContactAddress()
  {
    var useImport = new SiCabDeleteIsContactAddress.Import();
    var useExport = new SiCabDeleteIsContactAddress.Export();

    useImport.InterstateContact.StartDate =
      entities.InterstateContact.StartDate;
    useImport.InterstateRequest.IntHGeneratedId =
      import.InterstateRequest.IntHGeneratedId;

    Call(SiCabDeleteIsContactAddress.Execute, useImport, useExport);
  }

  private void DeleteInterstateContact()
  {
    Update("DeleteInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", entities.InterstateContact.IntGeneratedId);
          
        db.SetDate(
          command, "startDate",
          entities.InterstateContact.StartDate.GetValueOrDefault());
      });
  }

  private IEnumerable<bool> ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return ReadEach("ReadInterstateContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.Populated = true;

        return true;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    private InterstateRequest interstateRequest;
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

    private InterstateContact interstateContact;
    private InterstateRequest interstateRequest;
  }
#endregion
}
