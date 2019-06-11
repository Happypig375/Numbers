using System;
using PeterO.Numbers;

namespace Test {
  public static class EDecimalExtras {
    private const int DecimalRadix = 10;

    public static EDecimal Radix(EContext ec) {
      return EDecimal.FromInt32(DecimalRadix).RoundToPrecision(ec);
    }

    public static EDecimal Int32ToEDecimal(int i32, EContext ec) {
      // NOTE: Not a miscellaneous operation in the General Decimal
      // Arithmetic Specification 1.70, but required since some of the
      // miscellaneous operations here return integers
      return EDecimal.FromInt32(i32).RoundToPrecision(ec);
    }

    public static EDecimal BoolToEDecimal(bool b, EContext ec) {
      // NOTE: Not a miscellaneous operation in the General Decimal
      // Arithmetic Specification 1.70, but required since some of the
      // miscellaneous operations here return booleans
      return EDecimal.FromInt32(b ? 1 : 0).RoundToPrecision(ec);
    }

    public static bool IsCanonical(EDecimal ed) {
      return true;
    }

    public static bool IsFinite(EDecimal ed) {
      return ed.IsFinite;
    }

    public static bool IsInfinite(EDecimal ed) {
      return ed.IsInfinity();
    }

    public static bool IsNaN(EDecimal ed) {
      return ed != null && ed.IsNaN();
    }

    public static bool IsNormal(EDecimal ed, EContext ec) {
      return ed != null && ed.IsFinite && !ed.IsZero && !IsSubnormal(ed, ec);
    }

    public static bool IsQuietNaN(EDecimal ed) {
      return ed != null && ed.IsQuietNaN();
    }

    public static bool IsSigned(EDecimal ed) {
      return ed.IsNegative;
    }

    public static bool IsSignalingNaN(EDecimal ed) {
      return ed != null && ed.IsSignalingNaN();
    }

    private static readonly string[] NumberClasses = {
 "+Normal", "-Normal",
 "+Subnormal", "-Subnormal",
 "+Zero", "-Zero",
 "+Infinity", "-Infinity",
 "NaN", "sNaN"
};

    public static string NumberClassString(int nc) {
      if (nc < 0) {
   throw new ArgumentException("nc (" + nc +
          ") is not greater or equal to 0");
      }
      if (nc > 9) {
      throw new ArgumentException("nc (" + nc +
          ") is not less or equal to 9");
      }
      return NumberClasses[nc];
    }

    public static int NumberClass(EDecimal ed, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed.IsQuietNaN()) {
        return 8;
      }
      if (ed.IsNaN()) {
        return 9;
      }
      if (ed.IsInfinity()) {
        return ed.IsNegative ? 7 : 6;
      }
      if (ed.IsZero) {
        return ed.IsNegative ? 5 : 4;
      }
      return IsSubnormal(ed, ec) ? (ed.IsNegative ? 3 : 2) :
        (ed.IsNegative ? 1 : 0);
    }

    public static bool IsSubnormal(EDecimal ed, EContext ec) {
      if (ed.IsFinite && ec != null && !ed.IsZero && ec.HasExponentRange) {
        if (ec.AdjustExponent) {
          return ed.Exponent.Add(ed.Precision().Subtract(1)).CompareTo(
             ec.EMin) < 0;
        } else {
          return ed.Exponent.CompareTo(ec.EMin) < 0;
        }
      }
      return false;
    }

    public static bool IsZero(EDecimal ed) {
      return ed != null && ed.IsZero;
    }

    public static EDecimal LogB(EDecimal ed, EContext ec) {
      if (ed.IsNaN()) {
        return ed.RoundToPrecision(ec);
      }
      if (ed.IsInfinity()) {
        return EDecimal.PositiveInfinity;
      }
      if (ed.IsZero) {
        return EDecimal.FromInt32(-1).Divide(EDecimal.Zero, ec);
      }
      EInteger ei = ed.Exponent.Add(ed.Precision().Subtract(1));
      return EDecimal.FromEInteger(ei).RoundToPrecision(ec);
    }

    public static EDecimal ScaleB(EDecimal ed, EDecimal ed2, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed2 == null) {
        throw new ArgumentNullException(nameof(ed2));
      }
      if (ed.IsNaN() || ed2.IsNaN()) {
        return ed.Add(ed2, ec);
      }
      if (!ed2.IsFinite || ed2.Exponent.Sign != 0) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      EInteger scale = ed2.Mantissa;
      if (ec != null && ec.HasMaxPrecision) {
        EInteger exp = ec.EMax.Add(ec.Precision).Multiply(2);
        if (scale.Abs().CompareTo(exp.Abs()) > 0) {
          return InvalidOperation(EDecimal.NaN, ec);
        }
      }
      if (ed.IsInfinity()) {
        return ed;
      }
      if (scale.IsZero) {
        return ed.RoundToPrecision(ec);
      }
      EDecimal ret = EDecimal.Create(
         ed.UnsignedMantissa,
         ed.Exponent.Add(scale));
      if (ed.IsNegative) {
        ret = ret.Negate();
      }
      return ret.RoundToPrecision(ec);
    }

    public static EDecimal Shift(EDecimal ed, EDecimal ed2, EContext ec) {
      if (ed == null) {
        throw new ArgumentNullException(nameof(ed));
      }
      if (ed2 == null) {
        throw new ArgumentNullException(nameof(ed2));
      }
      if (ed.IsNaN() || ed2.IsNaN()) {
        return ed.Add(ed2, ec);
      }
      if (!ed2.IsFinite || ed2.Exponent.Sign != 0) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      EInteger shift = ed2.Mantissa;
      if (ec != null) {
        if (shift.Abs().CompareTo(ec.Precision) > 0) {
          return InvalidOperation(EDecimal.NaN, ec);
        }
      }
      if (ed.IsInfinity()) {
        // NOTE: Must check for validity of second
        // parameter first, before checking if first
        // parameter is infinity here
        return ed;
      }
      EInteger mant = ed.UnsignedMantissa;
      if (mant.IsZero) {
        return ed.RoundToPrecision(ec);
      }
      EInteger mantprec = ed.Precision();
      EInteger radix = EInteger.FromInt32(DecimalRadix);
      if (shift.Sign < 0) {
        if (shift.Abs().CompareTo(mantprec) < 0) {
          EInteger divisor = radix.Pow(shift.Abs());
          mant = mant.Divide(divisor);
        } else {
          mant = EInteger.Zero;
        }
        EDecimal ret = EDecimal.Create(mant, ed.Exponent);
        return ed.IsNegative ? ret.Negate() : ret;
      } else {
        EInteger mult = radix.Pow(shift);
        mant = mant.Multiply(mult);
        if (ec != null && ec.HasMaxPrecision) {
          EInteger mod = radix.Pow(ec.Precision);
          mant = mant.Remainder(mod);
        }
        EDecimal ret = EDecimal.Create(mant, ed.Exponent);
        return ed.IsNegative ? ret.Negate() : ret;
      }
    }

    public static EDecimal Rotate(EDecimal ed, EDecimal ed2, EContext ec) {
      if (ec == null || !ec.HasMaxPrecision) {
        return Shift(ed, ed2, ec);
      }
      if (ed.IsNaN() || ed2.IsNaN()) {
        return ed.Add(ed2, ec);
      }
      if (!ed2.IsFinite || ed2.Exponent.Sign != 0) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      EInteger shift = ed2.Mantissa;
      if (shift.Abs().CompareTo(ec.Precision) > 0) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      if (ed.IsInfinity()) {
        // NOTE: Must check for validity of second
        // parameter first, before checking if first
        // parameter is infinity here
        return ed;
      }
      EInteger mant = ed.UnsignedMantissa;
      EInteger mantprec = ed.Precision();
if (ec != null && ec.HasMaxPrecision && mantprec.CompareTo(ec.Precision) >
        0) {
     mant =
  mant.Remainder(EInteger.FromInt32(DecimalRadix).Pow(ec.Precision));
        mantprec = ec.Precision;
      }
      if (mant.IsZero) {
        return ed.RoundToPrecision(ec);
      }
      EInteger rightShift = shift.Sign < 0 ? shift.Abs() :
        ec.Precision.Subtract(shift);
      EInteger leftShift = ec.Precision.Subtract(rightShift);
      EInteger mantRight = EInteger.Zero;
      EInteger mantLeft = EInteger.Zero;
      EInteger radix = EInteger.FromInt32(DecimalRadix);
      // Right shift
      if (rightShift.CompareTo(mantprec) < 0) {
        EInteger divisor = radix.Pow(rightShift);
        mantRight = mant.Divide(divisor);
      } else {
        mantRight = EInteger.Zero;
      }
      // Left shift
      if (leftShift.IsZero) {
        mantLeft = mant;
      } else if (leftShift.CompareTo(ec.Precision) == 0) {
        mantLeft = EInteger.Zero;
      } else {
        EInteger mult = radix.Pow(leftShift);
        mantLeft = mant.Multiply(mult);
        EInteger mod = radix.Pow(ec.Precision);
        mantLeft = mantLeft.Remainder(mod);
      }
      EDecimal ret = EDecimal.Create(mantRight.Add(mantLeft), ed.Exponent);
      return ed.IsNegative ? ret.Negate() : ret;
    }

    public static int CompareTotal(EDecimal ed, EDecimal other, EContext ec) {
      return ed.CompareToTotal(other, ec);
    }

    public static int CompareTotalMagnitude(
  EDecimal ed,
  EDecimal other,
  EContext ec) {
      return ed.CompareToTotalMagnitude(other, ec);
    }

    public static EDecimal Copy(EDecimal ed) {
      return ed.Copy();
    }

    public static EDecimal Canonical(EDecimal ed) {
      return Copy(ed);
    }

    public static EDecimal CopyAbs(EDecimal ed) {
      return Copy(ed.Abs());
    }

    public static EDecimal CopyNegate(EDecimal ed) {
      return Copy(ed.Negate());
    }

    public static EDecimal CopySign(EDecimal ed, EDecimal other) {
      return ed.IsNegative == other.IsNegative ? Copy(ed) : CopyNegate(ed);
    }

    private static EDecimal InvalidOperation(EDecimal ed, EContext ec) {
      if (ec != null) {
        if (ec.HasFlags) {
          ec.Flags |= EContext.FlagInvalid;
        }
        if ((ec.Traps & EContext.FlagInvalid) != 0) {
          throw new ETrapException(EContext.FlagInvalid, ec, ed);
        }
      }
      return ed;
    }

    public static bool SameQuantum(EDecimal ed1, EDecimal ed2) {
      if (ed1 == null || ed2 == null) {
        return false;
      }
      if (ed1.IsFinite && ed2.IsFinite) {
        return ed1.Exponent.Equals(ed2.Exponent);
      } else {
 return (ed1.IsNaN() && ed2.IsNaN()) || (ed1.IsInfinity() &&
          ed2.IsInfinity());
      }
    }

    public static EDecimal Trim(EDecimal ed1, EContext ec) {
      EDecimal ed = ed1;
      if (ed1 == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      if (ed.IsSignalingNaN()) {
        return EDecimal.CreateNaN(
         ed.UnsignedMantissa,
         true,
         ed.IsNegative,
         ec);
      }
      if (ed.IsFinite) {
        if (ed.IsZero) {
          return (ed.IsNegative ? EDecimal.NegativeZero :
             EDecimal.Zero).RoundToPrecision(ec);
        } else if (ed.Exponent.Sign > 0) {
          return ed.Reduce(ec);
        } else if (ed.Exponent.Sign == 0) {
          return ed.RoundToPrecision(ec);
        } else {
          EInteger exp = ed.Exponent;
          EInteger mant = ed.UnsignedMantissa;
          bool neg = ed.IsNegative;
          var trimmed = false;
          EInteger radixint = EInteger.FromInt32(DecimalRadix);
          while (exp.Sign < 0 && mant.Sign > 0) {
            EInteger[] divrem = mant.DivRem(radixint);
            int rem = divrem[1].ToInt32Checked();
            if (rem != 0) {
              break;
            }
            mant = divrem[0];
            exp = exp.Add(1);
            trimmed = true;
          }
          if (!trimmed) {
            return ed.RoundToPrecision(ec);
          }
          EDecimal ret = EDecimal.Create(mant, exp);
          if (neg) {
            ret = ret.Negate();
          }
          return ret.RoundToPrecision(ec);
        }
      } else {
        return ed1.Plus(ec);
      }
    }

    public static EDecimal Rescale(EDecimal ed, EDecimal scale, EContext ec) {
      if (ed == null || scale == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      if (!scale.IsFinite) {
        return ed.Quantize(scale, ec);
      }
      if (scale.Exponent.IsZero) {
        return ed.Quantize(EDecimal.Create(EInteger.One, scale.Mantissa), ec);
      } else {
        EContext tec = ec == null ? null : ec.WithTraps(0).WithBlankFlags();
        EDecimal rv = scale.RoundToExponentExact(0, tec);
        if (!rv.IsFinite || (tec.Flags & EContext.FlagInexact) != 0) {
          if (ec != null && ec.IsSimplified) {
            // In simplified arithmetic, round scale to trigger
            // appropriate error conditions
            scale = scale.RoundToPrecision(ec);
          }
          return InvalidOperation(EDecimal.NaN, ec);
        }
        EDecimal rounded = scale.Quantize(0, tec);
        return ed.Quantize(
          EDecimal.Create(EInteger.One, rounded.Mantissa),
          ec);
      }
    }

    // Logical Operations
    public static EDecimal And(EDecimal ed1, EDecimal ed2, EContext ec) {
      byte[] logi1 = FromLogical(ed1, ec, 10);
      if (logi1 == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      byte[] logi2 = FromLogical(ed2, ec, 10);
      if (logi2 == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      byte[] smaller = logi1.Length < logi2.Length ? logi1 : logi2;
      byte[] bigger = logi1.Length < logi2.Length ? logi2 : logi1;
      for (var i = 0; i < smaller.Length; ++i) {
        smaller[i] &= bigger[i];
      }
      return EDecimal.FromEInteger(ToLogical(smaller, 10)).RoundToPrecision(ec);
    }

    public static EDecimal Invert(EDecimal ed1, EContext ec) {
      if (ec == null || !ec.HasMaxPrecision) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      // TODO: Make it work for bit precisions (e.g., .NET decimal)
      EInteger ei = EInteger.One.ShiftLeft(ec.Precision).Subtract(1);
      byte[] smaller = FromLogical(ed1, ec, 10);
      if (smaller == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      byte[] bigger = ei.ToBytes(true);
#if DEBUG
      if (smaller.Length > bigger.Length) {
        throw new ArgumentException("smaller.Length (" + smaller.Length +
          ") is not less or equal to " + bigger.Length);
      }
#endif

      for (var i = 0; i < smaller.Length; ++i) {
        bigger[i] ^= smaller[i];
      }
      return EDecimal.FromEInteger(ToLogical(bigger, 10)).RoundToPrecision(ec);
    }

    public static EDecimal Xor(EDecimal ed1, EDecimal ed2, EContext ec) {
      byte[] logi1 = FromLogical(ed1, ec, 10);
      if (logi1 == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      byte[] logi2 = FromLogical(ed2, ec, 10);
      if (logi2 == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      byte[] smaller = logi1.Length < logi2.Length ? logi1 : logi2;
      byte[] bigger = logi1.Length < logi2.Length ? logi2 : logi1;
      for (var i = 0; i < smaller.Length; ++i) {
        bigger[i] ^= smaller[i];
      }
      return EDecimal.FromEInteger(ToLogical(bigger, 10)).RoundToPrecision(ec);
    }

    public static EDecimal Or(EDecimal ed1, EDecimal ed2, EContext ec) {
      byte[] logi1 = FromLogical(ed1, ec, 10);
      if (logi1 == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      byte[] logi2 = FromLogical(ed2, ec, 10);
      if (logi2 == null) {
        return InvalidOperation(EDecimal.NaN, ec);
      }
      byte[] smaller = logi1.Length < logi2.Length ? logi1 : logi2;
      byte[] bigger = logi1.Length < logi2.Length ? logi2 : logi1;
      for (var i = 0; i < smaller.Length; ++i) {
        bigger[i] |= smaller[i];
      }
      return EDecimal.FromEInteger(ToLogical(bigger, 10)).RoundToPrecision(ec);
    }

    internal static EInteger ToLogical(byte[] bytes, int radix) {
      if (bytes == null) {
        throw new ArgumentNullException(nameof(bytes));
      }
      EInteger ret = EInteger.Zero;
      for (var i = bytes.Length - 1; i >= 0; --i) {
        int b = bytes[i];
        for (var j = 7; j >= 0; --j) {
          ret = ((bytes[i] & (1 << j)) != 0) ? ret.Multiply(radix).Add(1) :
               ret.Multiply(radix);
        }
      }
      return ret;
    }

    internal static byte[] FromLogical(EInteger um, EContext ec, int radix) {
      if (um == null || um.Sign < 0) {
 return null;
}
      if (um.Sign == 0) {
 return new byte[] { 0 };
}
      EInteger ret = EInteger.Zero;
      EInteger prec = um.GetDigitCountAsEInteger();
      // TODO: Make it work for bit precisions (e.g., .NET decimal)
      // while radix is other than 10
      EInteger maxprec = (ec != null && ec.HasMaxPrecision) ? ec.Precision :
           null;
      EInteger bytecount = prec.ShiftRight(3).Add(1);
      if (bytecount.CompareTo(0x7fffffff) > 0) {
        return null;  // Out of memory
      }
      var bitindex = 0;
      var bytes = new byte[bytecount.ToInt32Checked()];
      EInteger radixint = EInteger.FromInt32(radix);
      while (um.Sign > 0) {
        EInteger[] divrem = um.DivRem(radixint);
        int rem = divrem[1].ToInt32Checked();
        um = divrem[0];
        if (rem == 1) {
          // Don't collect bits beyond max precision
          if (maxprec == null || maxprec.CompareTo(bitindex) > 0) {
            int byteindex = bitindex >> 3;
            int mask = 1 << (bitindex & 7);
            bytes[byteindex] |= (byte)mask;
          }
        } else if (rem != 0) {
          return null;
        }
        ++bitindex;
      }
      return bytes;
    }

    internal static byte[] FromLogical(EDecimal ed, EContext ec, int radix) {
      return (!ed.IsFinite || ed.IsNegative || ed.Exponent.Sign != 0 ||
    ed.Mantissa.Sign < 0) ? null : FromLogical(ed.UnsignedMantissa, ec,
           radix);
    }

    internal static byte[] FromLogical(EFloat ed, EContext ec, int radix) {
      return (!ed.IsFinite || ed.IsNegative || ed.Exponent.Sign != 0 ||
    ed.Mantissa.Sign < 0) ? null : FromLogical(ed.UnsignedMantissa, ec,
           radix);
    }
  }
}
