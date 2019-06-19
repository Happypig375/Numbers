/*
Written by Peter O. in 2013.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */

using System;

namespace PeterO.Numbers {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="T:PeterO.Numbers.ERounding"]/*'/>
  public enum ERounding {
    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.None"]/*'/>
    None,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.Up"]/*'/>
    Up,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.Down"]/*'/>
    Down,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.HalfUp"]/*'/>
    HalfUp,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.HalfDown"]/*'/>
    HalfDown,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.HalfEven"]/*'/>
    HalfEven,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.Ceiling"]/*'/>
    Ceiling,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.Floor"]/*'/>
    Floor,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.Odd"]/*'/>
    [Obsolete("Consider using ERounding.OddOrZeroFiveUp instead.")]
    Odd,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.ZeroFiveUp"]/*'/>
    [Obsolete("Use ERounding.OddOrZeroFiveUp instead.")]
    ZeroFiveUp,

    /// <include file='../../docs.xml'
    /// path='docs/doc[@name="F:PeterO.Numbers.ERounding.OddOrZeroFiveUp"]/*'/>
    OddOrZeroFiveUp,
  }
}
