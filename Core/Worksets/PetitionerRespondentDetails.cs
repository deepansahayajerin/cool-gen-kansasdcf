// The source file: PETITIONER_RESPONDENT_DETAILS, ID: 371984444, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: LGLENFAC
/// </summary>
[Serializable]
public partial class PetitionerRespondentDetails: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PetitionerRespondentDetails()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PetitionerRespondentDetails(PetitionerRespondentDetails that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PetitionerRespondentDetails Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PetitionerRespondentDetails that)
  {
    base.Assign(that);
    petitionerName = that.petitionerName;
    morePetitionerInd = that.morePetitionerInd;
    respondentName = that.respondentName;
    moreRespondentInd = that.moreRespondentInd;
  }

  /// <summary>Length of the PETITIONER_NAME attribute.</summary>
  public const int PetitionerName_MaxLength = 33;

  /// <summary>
  /// The value of the PETITIONER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = PetitionerName_MaxLength)]
  public string PetitionerName
  {
    get => petitionerName ?? "";
    set => petitionerName =
      TrimEnd(Substring(value, 1, PetitionerName_MaxLength));
  }

  /// <summary>
  /// The json value of the PetitionerName attribute.</summary>
  [JsonPropertyName("petitionerName")]
  [Computed]
  public string PetitionerName_Json
  {
    get => NullIf(PetitionerName, "");
    set => PetitionerName = value;
  }

  /// <summary>Length of the MORE_PETITIONER_IND attribute.</summary>
  public const int MorePetitionerInd_MaxLength = 1;

  /// <summary>
  /// The value of the MORE_PETITIONER_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = MorePetitionerInd_MaxLength)]
  public string MorePetitionerInd
  {
    get => morePetitionerInd ?? "";
    set => morePetitionerInd =
      TrimEnd(Substring(value, 1, MorePetitionerInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MorePetitionerInd attribute.</summary>
  [JsonPropertyName("morePetitionerInd")]
  [Computed]
  public string MorePetitionerInd_Json
  {
    get => NullIf(MorePetitionerInd, "");
    set => MorePetitionerInd = value;
  }

  /// <summary>Length of the RESPONDENT_NAME attribute.</summary>
  public const int RespondentName_MaxLength = 33;

  /// <summary>
  /// The value of the RESPONDENT_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = RespondentName_MaxLength)]
  public string RespondentName
  {
    get => respondentName ?? "";
    set => respondentName =
      TrimEnd(Substring(value, 1, RespondentName_MaxLength));
  }

  /// <summary>
  /// The json value of the RespondentName attribute.</summary>
  [JsonPropertyName("respondentName")]
  [Computed]
  public string RespondentName_Json
  {
    get => NullIf(RespondentName, "");
    set => RespondentName = value;
  }

  /// <summary>Length of the MORE_RESPONDENT_IND attribute.</summary>
  public const int MoreRespondentInd_MaxLength = 1;

  /// <summary>
  /// The value of the MORE_RESPONDENT_IND attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = MoreRespondentInd_MaxLength)]
  public string MoreRespondentInd
  {
    get => moreRespondentInd ?? "";
    set => moreRespondentInd =
      TrimEnd(Substring(value, 1, MoreRespondentInd_MaxLength));
  }

  /// <summary>
  /// The json value of the MoreRespondentInd attribute.</summary>
  [JsonPropertyName("moreRespondentInd")]
  [Computed]
  public string MoreRespondentInd_Json
  {
    get => NullIf(MoreRespondentInd, "");
    set => MoreRespondentInd = value;
  }

  private string petitionerName;
  private string morePetitionerInd;
  private string respondentName;
  private string moreRespondentInd;
}
