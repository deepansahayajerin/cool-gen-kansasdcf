// Program: SI_GEN_CSE_PERSON_LICENSE_ID, ID: 371756203, model: 746.
// Short name: SWE01166
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_GEN_CSE_PERSON_LICENSE_ID.
/// </para>
/// <para>
/// RESP: SRVINIT	
/// This creates the number that uniquely identifies a license.
/// </para>
/// </summary>
[Serializable]
public partial class SiGenCsePersonLicenseId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GEN_CSE_PERSON_LICENSE_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGenCsePersonLicenseId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGenCsePersonLicenseId.
  /// </summary>
  public SiGenCsePersonLicenseId(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date	   Developer		Request #	Description
    // ??-??-??   ?????????		0	Initial Development
    // ---------------------------------------------------------
    // 10/06/99 W.Campbell        Added qualification to the
    //                            following READ EACH to only
    //                            READ for the license for the import
    //                            CSE_PERSON number.  The import
    //                            view for CSE_PERSON was also added.
    //                            Work performed for PR# H00076435.
    // ---------------------------------------------------------
    ReadCsePersonLicense();
    export.CsePersonLicense.Identifier =
      entities.CsePersonLicense.Identifier + 1;
  }

  private bool ReadCsePersonLicense()
  {
    entities.CsePersonLicense.Populated = false;

    return Read("ReadCsePersonLicense",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonLicense.Identifier = db.GetInt32(reader, 0);
        entities.CsePersonLicense.CspNumber = db.GetString(reader, 1);
        entities.CsePersonLicense.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    private CsePersonLicense csePersonLicense;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of CsePersonLicense.
    /// </summary>
    [JsonPropertyName("csePersonLicense")]
    public CsePersonLicense CsePersonLicense
    {
      get => csePersonLicense ??= new();
      set => csePersonLicense = value;
    }

    private CsePerson csePerson;
    private CsePersonLicense csePersonLicense;
  }
#endregion
}
