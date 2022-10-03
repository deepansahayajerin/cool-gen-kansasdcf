// Program: OE_FPLS_ADDRESS_SUBSTRING, ID: 372356113, model: 746.
// Short name: SWE00908
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
/// A program: OE_FPLS_ADDRESS_SUBSTRING.
/// </para>
/// <para>
/// ESP: OBLGESTB
/// This action block imports FPLS-LOCATE-RESPONSE Address and substring address
/// by '\' delimeter. Address-format-ind = 'C' record has city on position 157-
/// 181.
/// Address-format-ind = 'F' record has city within last two '\' in conjunction 
/// with street.  2 character state code is in position 182-183.
/// 9 digit zip code is on position 184-192.
/// </para>
/// </summary>
[Serializable]
public partial class OeFplsAddressSubstring: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FPLS_ADDRESS_SUBSTRING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFplsAddressSubstring(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFplsAddressSubstring.
  /// </summary>
  public OeFplsAddressSubstring(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // 05/24/00 M.Lachowicz  Fix FPLS screen.
    //                       PR 96045.
    if (IsEmpty(import.FplsLocateResponse.ReturnedAddress))
    {
      return;
    }

    // ************************************************
    // *Format F :The first 182 positions contain the *
    // *first 2 lines of Address seperated by "\"     *
    // ************************************************
    if (AsChar(import.FplsLocateResponse.AddressFormatInd) == 'F' || AsChar
      (import.FplsLocateResponse.AddressFormatInd) == 'C')
    {
      // -----------------------------------------------------------------
      // PR#79011
      // Beginning Of Change
      // ------------------------------------------------------------------
      local.NewResponse.AddrText =
        Substring(import.FplsLocateResponse.ReturnedAddress, 1, 160);

      // -----------------------------------------------------------------
      // PR#79011
      // End Of Change
      // ------------------------------------------------------------------
    }
    else if (AsChar(import.FplsLocateResponse.AddressFormatInd) == 'X')
    {
      local.FixedFormat.Count = 39;
      local.FplsLocateResponse.ReturnedAddress =
        import.FplsLocateResponse.ReturnedAddress ?? "";

      for(export.FplsAddress.Index = 0; export.FplsAddress.Index < 6; ++
        export.FplsAddress.Index)
      {
        if (!export.FplsAddress.CheckSize())
        {
          break;
        }

        export.FplsAddress.Update.FplsAddr.Text39 =
          Substring(local.FplsLocateResponse.ReturnedAddress, 1,
          local.FixedFormat.Count);
        local.LengthFixedFormat.Count =
          FplsLocateResponse.ReturnedAddress_MaxLength - 39;
        local.FplsLocateResponse.ReturnedAddress =
          Substring(local.FplsLocateResponse.ReturnedAddress,
          local.FixedFormat.Count + 1, local.LengthFixedFormat.Count);
        ++local.TotalCount.Count;

        if (local.TotalCount.Count >= 6)
        {
          return;
        }
      }

      export.FplsAddress.CheckIndex();
    }
    else
    {
      // 05/24/00 M.L Start
      if (Equal(import.FplsLocateResponse.AgencyCode, "H97") || Equal
        (import.FplsLocateResponse.AgencyCode, "H98") || Equal
        (import.FplsLocateResponse.AgencyCode, "H99"))
      {
        local.FixedFormat.Count = 39;
        local.FplsLocateResponse.ReturnedAddress =
          import.FplsLocateResponse.ReturnedAddress ?? "";

        for(export.FplsAddress.Index = 0; export.FplsAddress.Index < 6; ++
          export.FplsAddress.Index)
        {
          if (!export.FplsAddress.CheckSize())
          {
            break;
          }

          export.FplsAddress.Update.FplsAddr.Text39 =
            Substring(local.FplsLocateResponse.ReturnedAddress, 1,
            local.FixedFormat.Count);
          local.LengthFixedFormat.Count =
            FplsLocateResponse.ReturnedAddress_MaxLength - 39;
          local.FplsLocateResponse.ReturnedAddress =
            Substring(local.FplsLocateResponse.ReturnedAddress,
            local.FixedFormat.Count + 1, local.LengthFixedFormat.Count);
          ++local.TotalCount.Count;

          if (local.TotalCount.Count >= 6)
          {
            return;
          }
        }

        export.FplsAddress.CheckIndex();
      }

      // 05/24/00 M.L End
    }

    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // The following statement is commented out because for 'F' and for 'C' the 
    // initial length is same(160 bytes) so 'C' is also included in above 'If'
    // statement.
    // ------------------------------------------------------------------
    // ************************************************
    // *Format F :The first 182 positions contain the *
    // *first 3 lines of Address seperated by "\"     *
    // ************************************************
    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
    // ************************************************
    // *Now we must break down the Address format to  *
    // *seperate the lines of address delimited "\"   *
    // ************************************************
    local.EndPointer.Count = 0;

    for(export.FplsAddress.Index = 0; export.FplsAddress.Index < 6; ++
      export.FplsAddress.Index)
    {
      if (!export.FplsAddress.CheckSize())
      {
        break;
      }

      // ************************************************
      // *Locate each Back-Slash.                       *
      // ************************************************
      local.EndPointer.Count = Find(local.NewResponse.AddrText, "\\");

      // ************************************************
      // *When the End Pointer contains a Zero value    *
      // *after a find -  this indicates that there are *
      // *no more "\" to be found in the string. Note   *
      // *that at this point the current subscript value*
      // *contains no data.                             *
      // ************************************************
      if (local.EndPointer.Count == 0)
      {
        break;
      }

      // ************************************************
      // *Move a line of Address                        *
      // ************************************************
      // -----------------------------------------------------------------
      // PR#79011
      // Beginning Of Change
      // ------------------------------------------------------------------
      export.FplsAddress.Update.FplsAddr.Text39 =
        Substring(local.NewResponse.AddrText, 1, local.EndPointer.Count - 1);

      // -----------------------------------------------------------------
      // PR#79011
      // End Of Change
      // ------------------------------------------------------------------
      if (local.EndPointer.Count >= OblgWork.AddrText_MaxLength)
      {
        ExitState = "ACO_NI0000_MORE_ROWS_EXIST";

        return;
      }

      // ************************************************
      // *Set pointer to the next position after "\"    *
      // ************************************************
      local.NewResponse.AddrText =
        Substring(local.NewResponse.AddrText, local.EndPointer.Count +
        1, OblgWork.AddrText_MaxLength - local.EndPointer.Count);

      if (Length(TrimEnd(local.NewResponse.AddrText)) < 1)
      {
        break;
      }
    }

    export.FplsAddress.CheckIndex();

    // ************************************************
    // *Set the end pointer to the size of the actual *
    // *data returned in the address field - Max 192  *
    // ************************************************
    local.NewResponse.AddrLength =
      Length(TrimEnd(import.FplsLocateResponse.ReturnedAddress));

    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // Following statements are commented out and Zip code is set to actual 
    // length 15.
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // Following statements are commented out and state code is set to actual 
    // length 2.
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // Following statements are commented out and City is set to actual length 
    // 30.
    // ------------------------------------------------------------------
    local.FplsWorkArea.AddrZip =
      Substring(import.FplsLocateResponse.ReturnedAddress, 193, 15);

    if (AsChar(import.FplsLocateResponse.AddressFormatInd) == 'C')
    {
      local.FplsWorkArea.AddrCity =
        Substring(import.FplsLocateResponse.ReturnedAddress, 161, 30);
      local.FplsWorkArea.AddrSt =
        Substring(import.FplsLocateResponse.ReturnedAddress, 191, 2);

      ++export.FplsAddress.Index;
      export.FplsAddress.CheckSize();

      export.FplsAddress.Update.FplsAddr.Text39 =
        TrimEnd(local.FplsWorkArea.AddrCity);
    }

    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // Following statements are commented out and City is set to actual length 
    // 30.
    // ------------------------------------------------------------------
    if (!IsEmpty(local.FplsWorkArea.AddrSt))
    {
      local.FplsWorkArea.Text18 = local.FplsWorkArea.AddrSt;
    }

    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // Following statements are commented out and state code and City are 
    // concatinated.
    // ------------------------------------------------------------------
    if (!IsEmpty(local.FplsWorkArea.AddrZip))
    {
      local.FplsWorkArea.Text18 = TrimEnd(local.FplsWorkArea.Text18) + " " + local
        .FplsWorkArea.AddrZip;
    }

    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
    ++export.FplsAddress.Index;
    export.FplsAddress.CheckSize();

    // -----------------------------------------------------------------
    // PR#79011
    // Beginning Of Change
    // Following statements are commented out and concatinate last address line 
    // with state and Zip just created.
    // ------------------------------------------------------------------
    if (!IsEmpty(export.FplsAddress.Item.FplsAddr.Text39) && !
      IsEmpty(local.FplsWorkArea.Text18))
    {
      export.FplsAddress.Update.FplsAddr.Text39 =
        TrimEnd(export.FplsAddress.Item.FplsAddr.Text39) + ",  " + TrimEnd
        (local.FplsWorkArea.Text18);
    }
    else if (IsEmpty(export.FplsAddress.Item.FplsAddr.Text39) && !
      IsEmpty(local.FplsWorkArea.Text18))
    {
      export.FplsAddress.Update.FplsAddr.Text39 = local.FplsWorkArea.Text18;
    }

    // -----------------------------------------------------------------
    // PR#79011
    // End Of Change
    // ------------------------------------------------------------------
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    private FplsLocateResponse fplsLocateResponse;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A FplsAddressGroup group.</summary>
    [Serializable]
    public class FplsAddressGroup
    {
      /// <summary>
      /// A value of FplsAddr.
      /// </summary>
      [JsonPropertyName("fplsAddr")]
      public FplsWorkArea FplsAddr
      {
        get => fplsAddr ??= new();
        set => fplsAddr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea fplsAddr;
    }

    /// <summary>A AddressGroup group.</summary>
    [Serializable]
    public class AddressGroup
    {
      /// <summary>
      /// A value of AddrFplsWorkArea.
      /// </summary>
      [JsonPropertyName("addrFplsWorkArea")]
      public FplsWorkArea AddrFplsWorkArea
      {
        get => addrFplsWorkArea ??= new();
        set => addrFplsWorkArea = value;
      }

      /// <summary>
      /// A value of AddrWorkArea.
      /// </summary>
      [JsonPropertyName("addrWorkArea")]
      public WorkArea AddrWorkArea
      {
        get => addrWorkArea ??= new();
        set => addrWorkArea = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private FplsWorkArea addrFplsWorkArea;
      private WorkArea addrWorkArea;
    }

    /// <summary>
    /// Gets a value of FplsAddress.
    /// </summary>
    [JsonIgnore]
    public Array<FplsAddressGroup> FplsAddress => fplsAddress ??= new(
      FplsAddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of FplsAddress for json serialization.
    /// </summary>
    [JsonPropertyName("fplsAddress")]
    [Computed]
    public IList<FplsAddressGroup> FplsAddress_Json
    {
      get => fplsAddress;
      set => FplsAddress.Assign(value);
    }

    /// <summary>
    /// Gets a value of Address.
    /// </summary>
    [JsonIgnore]
    public Array<AddressGroup> Address => address ??= new(
      AddressGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Address for json serialization.
    /// </summary>
    [JsonPropertyName("address")]
    [Computed]
    public IList<AddressGroup> Address_Json
    {
      get => address;
      set => Address.Assign(value);
    }

    private Array<FplsAddressGroup> fplsAddress;
    private Array<AddressGroup> address;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TotalCount.
    /// </summary>
    [JsonPropertyName("totalCount")]
    public Common TotalCount
    {
      get => totalCount ??= new();
      set => totalCount = value;
    }

    /// <summary>
    /// A value of FplsLocateResponse.
    /// </summary>
    [JsonPropertyName("fplsLocateResponse")]
    public FplsLocateResponse FplsLocateResponse
    {
      get => fplsLocateResponse ??= new();
      set => fplsLocateResponse = value;
    }

    /// <summary>
    /// A value of LengthFixedFormat.
    /// </summary>
    [JsonPropertyName("lengthFixedFormat")]
    public Common LengthFixedFormat
    {
      get => lengthFixedFormat ??= new();
      set => lengthFixedFormat = value;
    }

    /// <summary>
    /// A value of FixedFormat.
    /// </summary>
    [JsonPropertyName("fixedFormat")]
    public Common FixedFormat
    {
      get => fixedFormat ??= new();
      set => fixedFormat = value;
    }

    /// <summary>
    /// A value of FplsWorkArea.
    /// </summary>
    [JsonPropertyName("fplsWorkArea")]
    public FplsWorkArea FplsWorkArea
    {
      get => fplsWorkArea ??= new();
      set => fplsWorkArea = value;
    }

    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of NewResponse.
    /// </summary>
    [JsonPropertyName("newResponse")]
    public OblgWork NewResponse
    {
      get => newResponse ??= new();
      set => newResponse = value;
    }

    /// <summary>
    /// A value of EndPointer.
    /// </summary>
    [JsonPropertyName("endPointer")]
    public Common EndPointer
    {
      get => endPointer ??= new();
      set => endPointer = value;
    }

    private Common totalCount;
    private FplsLocateResponse fplsLocateResponse;
    private Common lengthFixedFormat;
    private Common fixedFormat;
    private FplsWorkArea fplsWorkArea;
    private WorkArea workArea;
    private OblgWork newResponse;
    private Common endPointer;
  }
#endregion
}
