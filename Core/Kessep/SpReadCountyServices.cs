// Program: SP_READ_COUNTY_SERVICES, ID: 372328519, model: 746.
// Short name: SWE01404
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_READ_COUNTY_SERVICES.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// This action block validates the Office-Id passed and then gathers data 
/// related to services provided by office.
/// </para>
/// </summary>
[Serializable]
public partial class SpReadCountyServices: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_READ_COUNTY_SERVICES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpReadCountyServices(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpReadCountyServices.
  /// </summary>
  public SpReadCountyServices(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ********************************************
    // **  DATE      **  DESCRIPTION
    // **  05/19/95      R GREY
    // **  02/00/97	  R Grey  Add CTSV Function processing
    // *
    // *   04/30/97	  R Grey  Change Current Date
    // *   06/18/97	  R Grey  Fix Export List view match (delete hidden view
    // 			  stubs)
    // *  06/11/99   Anita Massey   changed read office
    //                           property to select only
    // *******************************************
    export.Search.SearchOffice1.Assign(import.SearchOffice.SearchOffice1);
    MoveOfficeAddress(import.SearchOffice.SearchOfficeAddress,
      export.Search.SearchOfficeAddress);
    export.Search.OldRec.Flag = import.SearchOffice.OldRec.Flag;
    export.SearchCntyPgm.SearchCseOrganization.Code =
      import.SearchCntyPgm.SearchCseOrganization.Code;
    export.SearchCntyPgm.SearchProgram.Code =
      import.SearchCntyPgm.SearchProgram.Code;
    export.SearchCntyPgm.SearchFunction.Function =
      import.SearchCntyPgm.SearchFunction.Function;
    export.SearchCntyPgm.SearchFuncOnly.Flag =
      import.SearchCntyPgm.SearchFuncOnly.Flag;
    local.Current.Date = Now().Date;

    if (ReadOffice())
    {
      export.Search.SearchOffice1.Assign(entities.Office);
    }
    else
    {
      ExitState = "FN0000_OFFICE_NF";

      return;
    }

    if (AsChar(export.Search.OldRec.Flag) == 'Y')
    {
      // ************************************************
      // * Display of discontinued County Services.
      // ************************************************
      if (AsChar(export.SearchCntyPgm.SearchFuncOnly.Flag) == 'Y')
      {
        // ************************************************
        // * Display request is for Function Type County Service only.
        // ************************************************
      }
      else
      {
        export.List.Index = 0;
        export.List.Clear();

        foreach(var item in ReadCountyServiceCseOrganizationProgram2())
        {
          if (!IsEmpty(export.SearchCntyPgm.SearchCseOrganization.Code))
          {
            if (!Lt(entities.CseOrganization.Code,
              export.SearchCntyPgm.SearchCseOrganization.Code))
            {
              if (!IsEmpty(export.SearchCntyPgm.SearchProgram.Code))
              {
                if (!Lt(entities.Program.Code,
                  export.SearchCntyPgm.SearchProgram.Code))
                {
                  // ********************************************
                  // * List discontinued County Services when
                  // * County Code search parameters = VALUE  and
                  // * Program Code search parameter = VALUE.
                  // ********************************************
                  export.List.Update.ListCountyService.Assign(
                    entities.CountyService);
                  export.List.Update.ListCountyService.Function = "";
                  export.List.Update.ListCseOrganization.Assign(
                    entities.CseOrganization);
                  export.List.Update.ListProgram.Assign(entities.Program);
                }
                else
                {
                  export.List.Next();

                  continue;
                }
              }

              // ********************************************
              // * List discontinued County Services when
              // * County Code search parameters = VALUE  and
              // * Program Code search parameter = spaces.
              // ********************************************
              export.List.Update.ListCountyService.
                Assign(entities.CountyService);
              export.List.Update.ListCountyService.Function = "";
              export.List.Update.ListCseOrganization.Assign(
                entities.CseOrganization);
              export.List.Update.ListProgram.Assign(entities.Program);
            }
            else
            {
              export.List.Next();

              continue;
            }
          }
          else
          {
            if (!IsEmpty(export.SearchCntyPgm.SearchProgram.Code))
            {
              if (!Lt(entities.Program.Code,
                export.SearchCntyPgm.SearchProgram.Code))
              {
                // ********************************************
                // * List discontinued County Services when
                // * County Code search parameters = SPACES and
                // * Program Code search parameter = VALUE.
                // ********************************************
                export.List.Update.ListCountyService.Assign(
                  entities.CountyService);
                export.List.Update.ListCountyService.Function = "";
                export.List.Update.ListCseOrganization.Assign(
                  entities.CseOrganization);
                export.List.Update.ListProgram.Assign(entities.Program);
              }
              else
              {
                export.List.Next();

                continue;
              }
            }

            // ********************************************
            // * List discontinued County Services when
            // * County Code search parameters = SPACES and
            // * Program Code search parameter = SPACES.
            // ********************************************
            export.List.Update.ListCountyService.Assign(entities.CountyService);
            export.List.Update.ListCountyService.Function = "";
            export.List.Update.ListCseOrganization.Assign(
              entities.CseOrganization);
            export.List.Update.ListProgram.Assign(entities.Program);
          }

          export.List.Next();
        }
      }

      // ********************************************
      // * Now Read for discontinued Function Type County Services.
      // ********************************************
      export.List.Index = export.List.Count;
      export.List.CheckIndex();

      foreach(var item in ReadCountyServiceCseOrganization2())
      {
        if (!IsEmpty(export.SearchCntyPgm.SearchCseOrganization.Code))
        {
          if (!Lt(entities.CseOrganization.Code,
            export.SearchCntyPgm.SearchCseOrganization.Code))
          {
            if (!IsEmpty(export.SearchCntyPgm.SearchFunction.Function))
            {
              if (!Lt(entities.CountyService.Function,
                export.SearchCntyPgm.SearchFunction.Function))
              {
                // ********************************************
                // * List discontinued County Services when
                // * County Code search parameters = VALUE and
                // * Function Code search parameter = VALUE.
                // ********************************************
                export.List.Update.ListCountyService.Assign(
                  entities.CountyService);
                export.List.Update.ListCseOrganization.Assign(
                  entities.CseOrganization);
                export.List.Update.ListProgram.Code = "";
              }
              else
              {
                export.List.Next();

                continue;
              }
            }
            else
            {
              // ********************************************
              // * List discontinued County Services when
              // * County Code search parameters = VALUE and
              // * Function Code search parameter = SPACES.
              // ********************************************
              export.List.Update.ListCountyService.
                Assign(entities.CountyService);
              export.List.Update.ListCseOrganization.Assign(
                entities.CseOrganization);
              export.List.Update.ListProgram.Code = "";
            }
          }
          else
          {
            export.List.Next();

            continue;
          }
        }
        else
        {
          if (!IsEmpty(export.SearchCntyPgm.SearchFunction.Function))
          {
            if (!Lt(entities.CountyService.Function,
              export.SearchCntyPgm.SearchFunction.Function))
            {
              // ********************************************
              // * List discontinued County Services when
              // * County Code search parameters = SPACES and
              // * Function Code search parameter = VALUE.
              // ********************************************
              export.List.Update.ListCountyService.
                Assign(entities.CountyService);
              export.List.Update.ListCseOrganization.Assign(
                entities.CseOrganization);
              export.List.Update.ListProgram.Code = "";
            }
            else
            {
              export.List.Next();

              continue;
            }
          }

          // ********************************************
          // * List discontinued County Services when
          // * County Code search parameters = SPACES and
          // * Function Code search parameter = SPACES.
          // ********************************************
          export.List.Update.ListCountyService.Assign(entities.CountyService);
          export.List.Update.ListCseOrganization.
            Assign(entities.CseOrganization);
          export.List.Update.ListProgram.Code = "";
        }

        export.List.Next();
      }
    }
    else
    {
      // ************************************************
      // * Display of active County Services.
      // ************************************************
      if (AsChar(export.SearchCntyPgm.SearchFuncOnly.Flag) == 'Y')
      {
        // ************************************************
        // * Display request is for Function Type County Service only.
        // ************************************************
      }
      else
      {
        export.List.Index = 0;
        export.List.Clear();

        foreach(var item in ReadCountyServiceCseOrganizationProgram1())
        {
          if (!IsEmpty(export.SearchCntyPgm.SearchCseOrganization.Code))
          {
            if (!Lt(entities.CseOrganization.Code,
              export.SearchCntyPgm.SearchCseOrganization.Code))
            {
              if (!IsEmpty(export.SearchCntyPgm.SearchProgram.Code))
              {
                if (!Lt(entities.Program.Code,
                  export.SearchCntyPgm.SearchProgram.Code))
                {
                  // ********************************************
                  // * List active County Services when
                  // * County Code search parameters = VALUE  and
                  // * Program Code search parameter = VALUE.
                  // ********************************************
                  export.List.Update.ListCountyService.Assign(
                    entities.CountyService);
                  export.List.Update.ListCountyService.Function = "";
                  export.List.Update.ListCseOrganization.Assign(
                    entities.CseOrganization);
                  export.List.Update.ListProgram.Assign(entities.Program);
                }
                else
                {
                  export.List.Next();

                  continue;
                }
              }

              // ********************************************
              // * List active County Services when
              // * County Code search parameters = VALUE  and
              // * Program Code search parameter = spaces.
              // ********************************************
              export.List.Update.ListCountyService.
                Assign(entities.CountyService);
              export.List.Update.ListCountyService.Function = "";
              export.List.Update.ListCseOrganization.Assign(
                entities.CseOrganization);
              export.List.Update.ListProgram.Assign(entities.Program);
            }
            else
            {
              export.List.Next();

              continue;
            }
          }
          else
          {
            if (!IsEmpty(export.SearchCntyPgm.SearchProgram.Code))
            {
              if (!Lt(entities.Program.Code,
                export.SearchCntyPgm.SearchProgram.Code))
              {
                // ********************************************
                // * List active County Services when
                // * County Code search parameters = SPACES and
                // * Program Code search parameter = VALUE.
                // ********************************************
                export.List.Update.ListCountyService.Assign(
                  entities.CountyService);
                export.List.Update.ListCountyService.Function = "";
                export.List.Update.ListCseOrganization.Assign(
                  entities.CseOrganization);
                export.List.Update.ListProgram.Assign(entities.Program);
              }
              else
              {
                export.List.Next();

                continue;
              }
            }

            // ********************************************
            // * List active County Services when
            // * County Code search parameters = SPACES and
            // * Program Code search parameter = SPACES.
            // ********************************************
            export.List.Update.ListCountyService.Assign(entities.CountyService);
            export.List.Update.ListCountyService.Function = "";
            export.List.Update.ListCseOrganization.Assign(
              entities.CseOrganization);
            export.List.Update.ListProgram.Assign(entities.Program);
          }

          export.List.Next();
        }
      }

      // ********************************************
      // * Now Read for Function Type County Services.
      // ********************************************
      export.List.Index = export.List.Count;
      export.List.CheckIndex();

      foreach(var item in ReadCountyServiceCseOrganization1())
      {
        if (!IsEmpty(export.SearchCntyPgm.SearchCseOrganization.Code))
        {
          if (!Lt(entities.CseOrganization.Code,
            export.SearchCntyPgm.SearchCseOrganization.Code))
          {
            if (!IsEmpty(export.SearchCntyPgm.SearchFunction.Function))
            {
              if (!Lt(entities.CountyService.Function,
                export.SearchCntyPgm.SearchFunction.Function))
              {
                // ********************************************
                // * List active County Services when
                // * County Code search parameters = VALUE and
                // * Function Code search parameter = VALUE.
                // ********************************************
                export.List.Update.ListCountyService.Assign(
                  entities.CountyService);
                export.List.Update.ListCseOrganization.Assign(
                  entities.CseOrganization);
                export.List.Update.ListProgram.Code = "";
              }
              else
              {
                export.List.Next();

                continue;
              }
            }

            // ********************************************
            // * List active County Services when
            // * County Code search parameters = VALUE and
            // * Function Code search parameter = SPACES.
            // ********************************************
            export.List.Update.ListCountyService.Assign(entities.CountyService);
            export.List.Update.ListCseOrganization.Assign(
              entities.CseOrganization);
            export.List.Update.ListProgram.Code = "";
          }
          else
          {
            export.List.Next();

            continue;
          }
        }
        else
        {
          if (!IsEmpty(export.SearchCntyPgm.SearchFunction.Function))
          {
            if (!Lt(entities.CountyService.Function,
              export.SearchCntyPgm.SearchFunction.Function))
            {
              // ********************************************
              // * List active County Services when
              // * County Code search parameters = SPACES and
              // * Function Code search parameter = VALUE.
              // ********************************************
              export.List.Update.ListCountyService.
                Assign(entities.CountyService);
              export.List.Update.ListCseOrganization.Assign(
                entities.CseOrganization);
              export.List.Update.ListProgram.Code = "";
            }
            else
            {
              export.List.Next();

              continue;
            }
          }

          // ********************************************
          // * List active County Services when
          // * County Code search parameters = SPACES and
          // * Function Code search parameter = SPACES.
          // ********************************************
          export.List.Update.ListCountyService.Assign(entities.CountyService);
          export.List.Update.ListCseOrganization.
            Assign(entities.CseOrganization);
          export.List.Update.ListProgram.Code = "";
        }

        export.List.Next();
      }
    }

    if (export.List.IsEmpty)
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
    else
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
  }

  private static void MoveOfficeAddress(OfficeAddress source,
    OfficeAddress target)
  {
    target.Type1 = source.Type1;
    target.City = source.City;
  }

  private IEnumerable<bool> ReadCountyServiceCseOrganization1()
  {
    return ReadEach("ReadCountyServiceCseOrganization1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.List.IsFull)
        {
          return false;
        }

        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 4);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 5);
        entities.CseOrganization.Type1 = db.GetString(reader, 5);
        entities.CountyService.CogCode = db.GetNullableString(reader, 6);
        entities.CseOrganization.Code = db.GetString(reader, 6);
        entities.CountyService.Function = db.GetNullableString(reader, 7);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 8);
        entities.CseOrganization.Name = db.GetString(reader, 9);
        entities.CseOrganization.Populated = true;
        entities.CountyService.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCountyServiceCseOrganization2()
  {
    return ReadEach("ReadCountyServiceCseOrganization2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.List.IsFull)
        {
          return false;
        }

        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 4);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 5);
        entities.CseOrganization.Type1 = db.GetString(reader, 5);
        entities.CountyService.CogCode = db.GetNullableString(reader, 6);
        entities.CseOrganization.Code = db.GetString(reader, 6);
        entities.CountyService.Function = db.GetNullableString(reader, 7);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 8);
        entities.CseOrganization.Name = db.GetString(reader, 9);
        entities.CseOrganization.Populated = true;
        entities.CountyService.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCountyServiceCseOrganizationProgram1()
  {
    return ReadEach("ReadCountyServiceCseOrganizationProgram1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.List.IsFull)
        {
          return false;
        }

        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 4);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 5);
        entities.CseOrganization.Type1 = db.GetString(reader, 5);
        entities.CountyService.CogCode = db.GetNullableString(reader, 6);
        entities.CseOrganization.Code = db.GetString(reader, 6);
        entities.CountyService.Function = db.GetNullableString(reader, 7);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 8);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.CseOrganization.Name = db.GetString(reader, 9);
        entities.Program.Code = db.GetString(reader, 10);
        entities.Program.DistributionProgramType = db.GetString(reader, 11);
        entities.CseOrganization.Populated = true;
        entities.CountyService.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCountyServiceCseOrganizationProgram2()
  {
    return ReadEach("ReadCountyServiceCseOrganizationProgram2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "offGeneratedId", entities.Office.SystemGeneratedId);
        db.SetDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.List.IsFull)
        {
          return false;
        }

        entities.CountyService.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CountyService.Type1 = db.GetString(reader, 1);
        entities.CountyService.EffectiveDate = db.GetDate(reader, 2);
        entities.CountyService.DiscontinueDate = db.GetDate(reader, 3);
        entities.CountyService.OffGeneratedId = db.GetNullableInt32(reader, 4);
        entities.CountyService.CogTypeCode = db.GetNullableString(reader, 5);
        entities.CseOrganization.Type1 = db.GetString(reader, 5);
        entities.CountyService.CogCode = db.GetNullableString(reader, 6);
        entities.CseOrganization.Code = db.GetString(reader, 6);
        entities.CountyService.Function = db.GetNullableString(reader, 7);
        entities.CountyService.PrgGeneratedId = db.GetNullableInt32(reader, 8);
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 8);
        entities.CseOrganization.Name = db.GetString(reader, 9);
        entities.Program.Code = db.GetString(reader, 10);
        entities.Program.DistributionProgramType = db.GetString(reader, 11);
        entities.CseOrganization.Populated = true;
        entities.CountyService.Populated = true;
        entities.Program.Populated = true;

        return true;
      });
  }

  private bool ReadOffice()
  {
    entities.Office.Populated = false;

    return Read("ReadOffice",
      (db, command) =>
      {
        db.SetInt32(
          command, "officeId",
          import.SearchOffice.SearchOffice1.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.Office.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.Office.TypeCode = db.GetString(reader, 1);
        entities.Office.Name = db.GetString(reader, 2);
        entities.Office.OffOffice = db.GetNullableInt32(reader, 3);
        entities.Office.Populated = true;
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
    /// <summary>A SearchCntyPgmGroup group.</summary>
    [Serializable]
    public class SearchCntyPgmGroup
    {
      /// <summary>
      /// A value of SearchProgram.
      /// </summary>
      [JsonPropertyName("searchProgram")]
      public Program SearchProgram
      {
        get => searchProgram ??= new();
        set => searchProgram = value;
      }

      /// <summary>
      /// A value of SearchCseOrganization.
      /// </summary>
      [JsonPropertyName("searchCseOrganization")]
      public CseOrganization SearchCseOrganization
      {
        get => searchCseOrganization ??= new();
        set => searchCseOrganization = value;
      }

      /// <summary>
      /// A value of SearchFunction.
      /// </summary>
      [JsonPropertyName("searchFunction")]
      public CountyService SearchFunction
      {
        get => searchFunction ??= new();
        set => searchFunction = value;
      }

      /// <summary>
      /// A value of SearchFuncOnly.
      /// </summary>
      [JsonPropertyName("searchFuncOnly")]
      public Common SearchFuncOnly
      {
        get => searchFuncOnly ??= new();
        set => searchFuncOnly = value;
      }

      private Program searchProgram;
      private CseOrganization searchCseOrganization;
      private CountyService searchFunction;
      private Common searchFuncOnly;
    }

    /// <summary>A SearchOfficeGroup group.</summary>
    [Serializable]
    public class SearchOfficeGroup
    {
      /// <summary>
      /// A value of OldRec.
      /// </summary>
      [JsonPropertyName("oldRec")]
      public Common OldRec
      {
        get => oldRec ??= new();
        set => oldRec = value;
      }

      /// <summary>
      /// A value of SearchOffice1.
      /// </summary>
      [JsonPropertyName("searchOffice1")]
      public Office SearchOffice1
      {
        get => searchOffice1 ??= new();
        set => searchOffice1 = value;
      }

      /// <summary>
      /// A value of SearchOfficeAddress.
      /// </summary>
      [JsonPropertyName("searchOfficeAddress")]
      public OfficeAddress SearchOfficeAddress
      {
        get => searchOfficeAddress ??= new();
        set => searchOfficeAddress = value;
      }

      private Common oldRec;
      private Office searchOffice1;
      private OfficeAddress searchOfficeAddress;
    }

    /// <summary>
    /// Gets a value of SearchCntyPgm.
    /// </summary>
    [JsonPropertyName("searchCntyPgm")]
    public SearchCntyPgmGroup SearchCntyPgm
    {
      get => searchCntyPgm ?? (searchCntyPgm = new());
      set => searchCntyPgm = value;
    }

    /// <summary>
    /// Gets a value of SearchOffice.
    /// </summary>
    [JsonPropertyName("searchOffice")]
    public SearchOfficeGroup SearchOffice
    {
      get => searchOffice ?? (searchOffice = new());
      set => searchOffice = value;
    }

    private SearchCntyPgmGroup searchCntyPgm;
    private SearchOfficeGroup searchOffice;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A SearchCntyPgmGroup group.</summary>
    [Serializable]
    public class SearchCntyPgmGroup
    {
      /// <summary>
      /// A value of SearchProgram.
      /// </summary>
      [JsonPropertyName("searchProgram")]
      public Program SearchProgram
      {
        get => searchProgram ??= new();
        set => searchProgram = value;
      }

      /// <summary>
      /// A value of SearchCseOrganization.
      /// </summary>
      [JsonPropertyName("searchCseOrganization")]
      public CseOrganization SearchCseOrganization
      {
        get => searchCseOrganization ??= new();
        set => searchCseOrganization = value;
      }

      /// <summary>
      /// A value of SearchFunction.
      /// </summary>
      [JsonPropertyName("searchFunction")]
      public CountyService SearchFunction
      {
        get => searchFunction ??= new();
        set => searchFunction = value;
      }

      /// <summary>
      /// A value of SearchFuncOnly.
      /// </summary>
      [JsonPropertyName("searchFuncOnly")]
      public Common SearchFuncOnly
      {
        get => searchFuncOnly ??= new();
        set => searchFuncOnly = value;
      }

      private Program searchProgram;
      private CseOrganization searchCseOrganization;
      private CountyService searchFunction;
      private Common searchFuncOnly;
    }

    /// <summary>A SearchGroup group.</summary>
    [Serializable]
    public class SearchGroup
    {
      /// <summary>
      /// A value of OldRec.
      /// </summary>
      [JsonPropertyName("oldRec")]
      public Common OldRec
      {
        get => oldRec ??= new();
        set => oldRec = value;
      }

      /// <summary>
      /// A value of SearchOffice1.
      /// </summary>
      [JsonPropertyName("searchOffice1")]
      public Office SearchOffice1
      {
        get => searchOffice1 ??= new();
        set => searchOffice1 = value;
      }

      /// <summary>
      /// A value of SearchOfficeAddress.
      /// </summary>
      [JsonPropertyName("searchOfficeAddress")]
      public OfficeAddress SearchOfficeAddress
      {
        get => searchOfficeAddress ??= new();
        set => searchOfficeAddress = value;
      }

      private Common oldRec;
      private Office searchOffice1;
      private OfficeAddress searchOfficeAddress;
    }

    /// <summary>A ListGroup group.</summary>
    [Serializable]
    public class ListGroup
    {
      /// <summary>
      /// A value of ListSel.
      /// </summary>
      [JsonPropertyName("listSel")]
      public Common ListSel
      {
        get => listSel ??= new();
        set => listSel = value;
      }

      /// <summary>
      /// A value of ListCnty.
      /// </summary>
      [JsonPropertyName("listCnty")]
      public Common ListCnty
      {
        get => listCnty ??= new();
        set => listCnty = value;
      }

      /// <summary>
      /// A value of ListPgm.
      /// </summary>
      [JsonPropertyName("listPgm")]
      public Common ListPgm
      {
        get => listPgm ??= new();
        set => listPgm = value;
      }

      /// <summary>
      /// A value of ListFunction.
      /// </summary>
      [JsonPropertyName("listFunction")]
      public Common ListFunction
      {
        get => listFunction ??= new();
        set => listFunction = value;
      }

      /// <summary>
      /// A value of ListCseOrganization.
      /// </summary>
      [JsonPropertyName("listCseOrganization")]
      public CseOrganization ListCseOrganization
      {
        get => listCseOrganization ??= new();
        set => listCseOrganization = value;
      }

      /// <summary>
      /// A value of ListCountyService.
      /// </summary>
      [JsonPropertyName("listCountyService")]
      public CountyService ListCountyService
      {
        get => listCountyService ??= new();
        set => listCountyService = value;
      }

      /// <summary>
      /// A value of ListProgram.
      /// </summary>
      [JsonPropertyName("listProgram")]
      public Program ListProgram
      {
        get => listProgram ??= new();
        set => listProgram = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 150;

      private Common listSel;
      private Common listCnty;
      private Common listPgm;
      private Common listFunction;
      private CseOrganization listCseOrganization;
      private CountyService listCountyService;
      private Program listProgram;
    }

    /// <summary>
    /// Gets a value of SearchCntyPgm.
    /// </summary>
    [JsonPropertyName("searchCntyPgm")]
    public SearchCntyPgmGroup SearchCntyPgm
    {
      get => searchCntyPgm ?? (searchCntyPgm = new());
      set => searchCntyPgm = value;
    }

    /// <summary>
    /// Gets a value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public SearchGroup Search
    {
      get => search ?? (search = new());
      set => search = value;
    }

    /// <summary>
    /// Gets a value of List.
    /// </summary>
    [JsonIgnore]
    public Array<ListGroup> List => list ??= new(ListGroup.Capacity);

    /// <summary>
    /// Gets a value of List for json serialization.
    /// </summary>
    [JsonPropertyName("list")]
    [Computed]
    public IList<ListGroup> List_Json
    {
      get => list;
      set => List.Assign(value);
    }

    private SearchCntyPgmGroup searchCntyPgm;
    private SearchGroup search;
    private Array<ListGroup> list;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of CountyService.
    /// </summary>
    [JsonPropertyName("countyService")]
    public CountyService CountyService
    {
      get => countyService ??= new();
      set => countyService = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    private CseOrganization cseOrganization;
    private Office office;
    private OfficeAddress officeAddress;
    private CountyService countyService;
    private Program program;
  }
#endregion
}
