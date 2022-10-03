// Program: EAB_UPDATE_NON_CASE_PERSON, ID: 372255282, model: 746.
// Short name: SWEXIU15
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_UPDATE_NON_CASE_PERSON.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This is an EAB stub to interface with the ADABAS files and update the CSE 
/// PERSON details
/// </para>
/// </summary>
[Serializable]
public partial class EabUpdateNonCasePerson: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_UPDATE_NON_CASE_PERSON program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabUpdateNonCasePerson(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabUpdateNonCasePerson.
  /// </summary>
  public EabUpdateNonCasePerson(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXIU15", context, import, export, EabOptions.Hpvp |
      EabOptions.NoIefParams);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    [Member(Index = 1, Members = new[] { "Date" })]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 2, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial",
      "Ssn",
      "Dob",
      "Sex",
      "Number"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Converted.
    /// </summary>
    [JsonPropertyName("converted")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
    public Common Converted
    {
      get => converted ??= new();
      set => converted = value;
    }

    private DateWorkArea current;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common converted;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    [Member(Index = 1, Members = new[]
    {
      "Type1",
      "AdabasFileNumber",
      "AdabasFileAction",
      "AdabasResponseCd",
      "CicsResourceNm",
      "CicsFunctionCd",
      "CicsResponseCd"
    })]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private AbendData abendData;
  }
#endregion
}
