// The source file: PERSON_GENETIC_TEST, ID: 371439262, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// The person who has already had a previous genetic test (blood test) within a
/// timeframe that can be reused for another schedule Genetic test.
/// </summary>
[Serializable]
public partial class PersonGeneticTest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PersonGeneticTest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PersonGeneticTest(PersonGeneticTest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PersonGeneticTest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PersonGeneticTest that)
  {
    base.Assign(that);
    specimenId = that.specimenId;
    identifier = that.identifier;
    sampleUsableInd = that.sampleUsableInd;
    collectSampleInd = that.collectSampleInd;
    showInd = that.showInd;
    sampleCollectedInd = that.sampleCollectedInd;
    scheduledTestTime = that.scheduledTestTime;
    scheduledTestDate = that.scheduledTestDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    cspNumber = that.cspNumber;
    gteTestNumber = that.gteTestNumber;
    venIdentifier = that.venIdentifier;
    pgtIdentifier = that.pgtIdentifier;
    cspRNumber = that.cspRNumber;
    gteRTestNumber = that.gteRTestNumber;
  }

  /// <summary>Length of the SPECIMEN_ID attribute.</summary>
  public const int SpecimenId_MaxLength = 10;

  /// <summary>
  /// The value of the SPECIMEN_ID attribute.
  /// Specimen ID given by Test Site (Roche).
  /// This is known at the time of receipt of test result only.
  /// </summary>
  [JsonPropertyName("specimenId")]
  [Member(Index = 1, Type = MemberType.Char, Length = SpecimenId_MaxLength, Optional
    = true)]
  public string SpecimenId
  {
    get => specimenId;
    set => specimenId = value != null
      ? TrimEnd(Substring(value, 1, SpecimenId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique number that descripts a particular line within the screen.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the SAMPLE_USABLE_IND attribute.</summary>
  public const int SampleUsableInd_MaxLength = 1;

  /// <summary>
  /// The value of the SAMPLE_USABLE_IND attribute.
  /// Specifies whether or not the sample was usable.
  /// </summary>
  [JsonPropertyName("sampleUsableInd")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = SampleUsableInd_MaxLength, Optional = true)]
  public string SampleUsableInd
  {
    get => sampleUsableInd;
    set => sampleUsableInd = value != null
      ? TrimEnd(Substring(value, 1, SampleUsableInd_MaxLength)) : null;
  }

  /// <summary>Length of the COLLECT_SAMPLE_IND attribute.</summary>
  public const int CollectSampleInd_MaxLength = 1;

  /// <summary>
  /// The value of the COLLECT_SAMPLE_IND attribute.
  /// 'Y' or 'N' to indicate whether Test Sample is
  /// needed or already available.
  /// </summary>
  [JsonPropertyName("collectSampleInd")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = CollectSampleInd_MaxLength, Optional = true)]
  public string CollectSampleInd
  {
    get => collectSampleInd;
    set => collectSampleInd = value != null
      ? TrimEnd(Substring(value, 1, CollectSampleInd_MaxLength)) : null;
  }

  /// <summary>Length of the SHOW_IND attribute.</summary>
  public const int ShowInd_MaxLength = 1;

  /// <summary>
  /// The value of the SHOW_IND attribute.
  /// Specifies whether or not the CSE Person showed up for the sample 
  /// collection.
  /// Y - CSE Person showed up for the test
  /// N - CSE Person did not show up for the test
  /// </summary>
  [JsonPropertyName("showInd")]
  [Member(Index = 5, Type = MemberType.Char, Length = ShowInd_MaxLength, Optional
    = true)]
  public string ShowInd
  {
    get => showInd;
    set => showInd = value != null
      ? TrimEnd(Substring(value, 1, ShowInd_MaxLength)) : null;
  }

  /// <summary>Length of the SAMPLE_COLLECTED_IND attribute.</summary>
  public const int SampleCollectedInd_MaxLength = 1;

  /// <summary>
  /// The value of the SAMPLE_COLLECTED_IND attribute.
  /// Confirmation from LAB that person showed up and Test Sample was taken on 
  /// scheduled date.
  /// Also indicates 'NO SHOW'.
  /// </summary>
  [JsonPropertyName("sampleCollectedInd")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = SampleCollectedInd_MaxLength, Optional = true)]
  public string SampleCollectedInd
  {
    get => sampleCollectedInd;
    set => sampleCollectedInd = value != null
      ? TrimEnd(Substring(value, 1, SampleCollectedInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SCHEDULED_TEST_TIME attribute.
  /// Time that person should be present at Test
  /// Sample Draw Site.
  /// </summary>
  [JsonPropertyName("scheduledTestTime")]
  [Member(Index = 7, Type = MemberType.Time, Optional = true)]
  public TimeSpan? ScheduledTestTime
  {
    get => scheduledTestTime;
    set => scheduledTestTime = value;
  }

  /// <summary>
  /// The value of the SCHEDULED_TEST_DATE attribute.
  /// Date set up by CSE with LAB that person should report to draw Genetic Test
  /// Sample.
  /// (ex: Blood test, Buccal Swab)
  /// </summary>
  [JsonPropertyName("scheduledTestDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? ScheduledTestDate
  {
    get => scheduledTestDate;
    set => scheduledTestDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 10, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 12, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>
  /// The value of the TEST_NUMBER attribute.
  /// A unique identifier of a genetic test.
  /// </summary>
  [JsonPropertyName("gteTestNumber")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 8)]
  public int GteTestNumber
  {
    get => gteTestNumber;
    set => gteTestNumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique system generated number that descripts the details of a particular 
  /// vendor.
  /// </summary>
  [JsonPropertyName("venIdentifier")]
  [Member(Index = 15, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? VenIdentifier
  {
    get => venIdentifier;
    set => venIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Unique number that descripts a particular line within the screen.
  /// </summary>
  [JsonPropertyName("pgtIdentifier")]
  [Member(Index = 16, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? PgtIdentifier
  {
    get => pgtIdentifier;
    set => pgtIdentifier = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspRNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonPropertyName("cspRNumber")]
  [Member(Index = 17, Type = MemberType.Char, Length = CspRNumber_MaxLength, Optional
    = true)]
  public string CspRNumber
  {
    get => cspRNumber;
    set => cspRNumber = value != null
      ? TrimEnd(Substring(value, 1, CspRNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the TEST_NUMBER attribute.
  /// A unique identifier of a genetic test.
  /// </summary>
  [JsonPropertyName("gteRTestNumber")]
  [Member(Index = 18, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? GteRTestNumber
  {
    get => gteRTestNumber;
    set => gteRTestNumber = value;
  }

  private string specimenId;
  private int identifier;
  private string sampleUsableInd;
  private string collectSampleInd;
  private string showInd;
  private string sampleCollectedInd;
  private TimeSpan? scheduledTestTime;
  private DateTime? scheduledTestDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string cspNumber;
  private int gteTestNumber;
  private int? venIdentifier;
  private int? pgtIdentifier;
  private string cspRNumber;
  private int? gteRTestNumber;
}
