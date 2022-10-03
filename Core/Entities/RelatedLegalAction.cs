// The source file: RELATED_LEGAL_ACTION, ID: 374600784, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// This entity contains the Legal Action Numbers that are related to the 
/// original Legal Action Document.  The original legal action document will
/// have the Court Case Number assigned to it when it is filed with the
/// tribunal.  This is needed because a Court Case Number, which is what the
/// documents are related by, is not assigned until the document is filed with a
/// tribunal.  LEGAL ACTION is created before the Court Case Number is
/// assigned.
/// </summary>
[Serializable]
public partial class RelatedLegalAction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public RelatedLegalAction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public RelatedLegalAction(RelatedLegalAction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new RelatedLegalAction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(RelatedLegalAction that)
  {
    base.Assign(that);
    identifier = that.identifier;
    lgaIdSucc = that.lgaIdSucc;
    lgaIdPrec = that.lgaIdPrec;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdSucc")]
  [Member(Index = 2, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdSucc
  {
    get => lgaIdSucc;
    set => lgaIdSucc = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdPrec")]
  [Member(Index = 3, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? LgaIdPrec
  {
    get => lgaIdPrec;
    set => lgaIdPrec = value;
  }

  private int identifier;
  private int? lgaIdSucc;
  private int? lgaIdPrec;
}
