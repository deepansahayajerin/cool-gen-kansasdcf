// The source file: PROGRAM_PROCESSING_INFO, ID: 371439867, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Contains the process date and other parameters for a specific program.  It 
/// is identified by the program name.  A CICS transaction may be used to update
/// the process date and parameters for each program.
/// </summary>
[Serializable]
public partial class ProgramProcessingInfo: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProgramProcessingInfo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProgramProcessingInfo(ProgramProcessingInfo that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProgramProcessingInfo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProgramProcessingInfo that)
  {
    base.Assign(that);
    name = that.name;
    processDate = that.processDate;
    parameterList = that.parameterList;
    description = that.description;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 8;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the program (from the exec card statement in the JCL) that is 
  /// to be run.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>
  /// The value of the PROCESS_DATE attribute.
  /// May contain the process date that should be used for this program.  A 
  /// program that runs daily may not use this date but instead get the process
  /// date from the current date.
  /// Example:
  /// We may run a program on April 4th with a process date of March 31st.  This
  /// program would then only process records on or up to march 31st.
  /// </summary>
  [JsonPropertyName("processDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? ProcessDate
  {
    get => processDate;
    set => processDate = value;
  }

  /// <summary>Length of the PARAMETER_LIST attribute.</summary>
  public const int ParameterList_MaxLength = 240;

  /// <summary>
  /// The value of the PARAMETER_LIST attribute.
  /// This attribute contains a parameter line that may be made up of one or 
  /// more program input parameters.  Each program will have to interrogate the
  /// parameter line that is being passed to it and break it into it's separate
  /// components.
  /// </summary>
  [JsonPropertyName("parameterList")]
  [Member(Index = 3, Type = MemberType.Varchar, Length
    = ParameterList_MaxLength, Optional = true)]
  public string ParameterList
  {
    get => parameterList;
    set => parameterList = value != null
      ? Substring(value, 1, ParameterList_MaxLength) : null;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// A description of the program and any other information that pertains to 
  /// running the program.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 4, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  private string name;
  private DateTime? processDate;
  private string parameterList;
  private string description;
  private string createdBy;
  private DateTime? createdTimestamp;
}
