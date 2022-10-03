﻿// Program: EAB_WRITE_PATERNITY_EXTRACT, ID: 371170016, model: 746.
// Short name: SWEXPE07
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_WRITE_PATERNITY_EXTRACT.
/// </para>
/// <para>
/// Used to open, write to, and close the paternity report extract file.  This 
/// extract is produced in SRRUN276.
/// </para>
/// </summary>
[Serializable]
public partial class EabWritePaternityExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_WRITE_PATERNITY_EXTRACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabWritePaternityExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabWritePaternityExtract.
  /// </summary>
  public EabWritePaternityExtract(IContext context, Import import, Export export)
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
    GetService<IEabStub>().Execute(
      "SWEXPE07", context, import, export, EabOptions.Hpvp);
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
    /// A value of NameArea.
    /// </summary>
    [JsonPropertyName("nameArea")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Name" })]
    public Office NameArea
    {
      get => nameArea ??= new();
      set => nameArea = value;
    }

    /// <summary>
    /// A value of Name.
    /// </summary>
    [JsonPropertyName("name")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Name" })]
    public Office Name
    {
      get => name ??= new();
      set => name = value;
    }

    /// <summary>
    /// A value of SupervisorName.
    /// </summary>
    [JsonPropertyName("supervisorName")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "LastName", "FirstName" })]
    public ServiceProvider SupervisorName
    {
      get => supervisorName ??= new();
      set => supervisorName = value;
    }

    /// <summary>
    /// A value of Co.
    /// </summary>
    [JsonPropertyName("co")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "LastName", "FirstName" })]
    public ServiceProvider Co
    {
      get => co ??= new();
      set => co = value;
    }

    /// <summary>
    /// A value of Priority4Category.
    /// </summary>
    [JsonPropertyName("priority4Category")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "SelectChar" })]
    public Common Priority4Category
    {
      get => priority4Category ??= new();
      set => priority4Category = value;
    }

    /// <summary>
    /// A value of PriorityNumber.
    /// </summary>
    [JsonPropertyName("priorityNumber")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "SelectChar" })]
    public Common PriorityNumber
    {
      get => priorityNumber ??= new();
      set => priorityNumber = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ch.
    /// </summary>
    [JsonPropertyName("ch")]
    [Member(Index = 9, AccessFields = false, Members = new[]
    {
      "Type1",
      "Number",
      "BirthPlaceState"
    })]
    public CsePerson Ch
    {
      get => ch ??= new();
      set => ch = value;
    }

    /// <summary>
    /// A value of ChEDate.
    /// </summary>
    [JsonPropertyName("chEDate")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "Type1", "DateOfEmancipation" })]
    public CaseRole ChEDate
    {
      get => chEDate ??= new();
      set => chEDate = value;
    }

    /// <summary>
    /// A value of ApName.
    /// </summary>
    [JsonPropertyName("apName")]
    [Member(Index = 11, AccessFields = false, Members = new[]
    {
      "FirstName",
      "MiddleInitial",
      "LastName",
      "Sex"
    })]
    public CsePersonsWorkSet ApName
    {
      get => apName ??= new();
      set => apName = value;
    }

    /// <summary>
    /// A value of ChName.
    /// </summary>
    [JsonPropertyName("chName")]
    [Member(Index = 12, AccessFields = false, Members = new[]
    {
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet ChName
    {
      get => chName ??= new();
      set => chName = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Office nameArea;
    private Office name;
    private ServiceProvider supervisorName;
    private ServiceProvider co;
    private Common priority4Category;
    private Common priorityNumber;
    private Case1 case1;
    private CsePerson ap;
    private CsePerson ch;
    private CaseRole chEDate;
    private CsePersonsWorkSet apName;
    private CsePersonsWorkSet chName;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
