/*
Written by Peter O. in 2014-2016.
Any copyright is dedicated to the Public Domain.
http://creativecommons.org/publicdomain/zero/1.0/
If you like this, you should donate to Peter O.
at: http://peteroupc.github.io/
 */
using System;
using System.Text;
using PeterO;
using PeterO.Numbers;

namespace Test {
  /// <summary>Description of RandomObjects.</summary>
  public static class RandomObjects {
    private const int MaxExclusiveStringLength = 0x2000;
    private const int MaxExclusiveShortStringLength = 50;
    private const int MaxNumberLength = 50000;
    private const int MaxShortNumberLength = 40;

    public static byte[] RandomByteString(IRandomGenExtended rand) {
      if (rand == null) {
        throw new ArgumentNullException(nameof(rand));
      }
      int x = rand.GetInt32(MaxExclusiveStringLength);
      var bytes = new byte[x];
      rand.GetBytes(bytes, 0, bytes.Length);
      return bytes;
    }

    public static byte[] RandomByteString(IRandomGenExtended rand, int length) {
      if (rand == null) {
        throw new ArgumentNullException(nameof(rand));
      }
      var bytes = new byte[length];
      rand.GetBytes(bytes, 0, bytes.Length);
      return bytes;
    }

    public static byte[] RandomByteStringShort(IRandomGenExtended rand) {
      if (rand == null) {
        throw new ArgumentNullException(nameof(rand));
      }
      return RandomByteString(
        rand,
        rand.GetInt32(MaxExclusiveShortStringLength));
    }

    public static ERational RandomERational(IRandomGenExtended rand) {
      EInteger bigintA = RandomEInteger(rand);
      EInteger bigintB = RandomEInteger(rand);
      if (bigintB.IsZero) {
        bigintB = EInteger.One;
      }
      return ERational.Create(bigintA, bigintB);
    }

    public static string RandomTextString(IRandomGenExtended rand) {
      if (rand == null) {
        throw new ArgumentNullException(nameof(rand));
      }
      int length = rand.GetInt32(MaxExclusiveStringLength);
      var sb = new StringBuilder();
      for (var i = 0; i < length; ++i) {
        int x = rand.GetInt32(100);
        if (x < 95) {
          // ASCII
          sb.Append((char)(0x20 + rand.GetInt32(0x60)));
        } else if (x < 98) {
          // Supplementary character
          x = rand.GetInt32(0x400) + 0xd800;
          sb.Append((char)x);
          x = rand.GetInt32(0x400) + 0xdc00;
          sb.Append((char)x);
        } else {
          // BMP character
          x = 0x20 + rand.GetInt32(0xffe0);
          if (x >= 0xd800 && x < 0xe000) {
            // surrogate code unit, generate ASCII instead
            x = 0x20 + rand.GetInt32(0x60);
          }
          sb.Append((char)x);
        }
      }
      return sb.ToString();
    }

    public static int RandomInt32(IRandomGenExtended rand) {
      byte[] bytes = RandomByteString(rand, 4);
      int ret = ((int)bytes[0]) & 0xff;
      ret |= (((int)bytes[1]) & 0xff) << 8;
      ret |= (((int)bytes[2]) & 0xff) << 16;
      ret |= (((int)bytes[3]) & 0xff) << 24;
      return ret;
    }

    public static long RandomInt64(IRandomGenExtended rand) {
      byte[] bytes = RandomByteString(rand, 8);
      long ret = ((long)bytes[0]) & 0xff;
      ret |= (((long)bytes[1]) & 0xff) << 8;
      ret |= (((long)bytes[2]) & 0xff) << 16;
      ret |= (((long)bytes[3]) & 0xff) << 24;
      ret |= (((long)bytes[4]) & 0xff) << 32;
      ret |= (((long)bytes[5]) & 0xff) << 40;
      ret |= (((long)bytes[6]) & 0xff) << 48;
      ret |= (((long)bytes[7]) & 0xff) << 56;
      return ret;
    }

    public static double RandomDouble(IRandomGenExtended rand, int exponent) {
      if (exponent == Int32.MaxValue) {
        if (rand == null) {
          throw new ArgumentNullException(nameof(rand));
        }
        exponent = rand.GetInt32(2047);
      }
      if (rand == null) {
        throw new ArgumentNullException(nameof(rand));
      }
      long r = rand.GetInt32(0x10000);
      r |= ((long)rand.GetInt32(0x10000)) << 16;
      if (rand.GetInt32(2) == 0) {
        r |= ((long)rand.GetInt32(0x10000)) << 32;
        if (rand.GetInt32(2) == 0) {
          r |= ((long)rand.GetInt32(0x10000)) << 48;
        }
      }
      r &= ~0x7ff0000000000000L; // clear exponent
      r |= ((long)exponent) << 52; // set exponent
      return BitConverter.ToDouble(BitConverter.GetBytes((long)r), 0);
    }

    public static double RandomDouble(IRandomGenExtended rand) {
      long r = RandomInt64(rand);
      return BitConverter.ToDouble(BitConverter.GetBytes((long)r), 0);
    }

    public static float RandomSingle(IRandomGenExtended rand) {
      int r = RandomInt32(rand);
      return BitConverter.ToSingle(BitConverter.GetBytes((int)r), 0);
    }

    public static float RandomSingle(IRandomGenExtended rand, int exponent) {
      if (exponent == Int32.MaxValue) {
        if (rand == null) {
          throw new ArgumentNullException(nameof(rand));
        }
        exponent = rand.GetInt32(255);
      }
      if (rand == null) {
        throw new ArgumentNullException(nameof(rand));
      }
      int r = rand.GetInt32(0x10000);
      if (rand.GetInt32(2) == 0) {
        r |= ((int)rand.GetInt32(0x10000)) << 16;
      }
      r &= ~0x7f800000; // clear exponent
      r |= ((int)exponent) << 23; // set exponent
      return BitConverter.ToSingle(BitConverter.GetBytes((int)r), 0);
    }

    private static string GenerateEDecimalSmallString(IRandomGenExtended
wrapper, bool extended) {
       var sb = new StringBuilder();
       int len = 1 + wrapper.GetInt32(4);
       if (!extended) {
        sb.Append((char)('1' + wrapper.GetInt32(9)));
        --len;
       }
       AppendRandomDecimals(wrapper, sb, len);
       sb.Append('.');
       len = 1 + wrapper.GetInt32(36);
       AppendRandomDecimals(wrapper, sb, len);
       sb.Append('E');
       len = wrapper.GetInt32(25) - 12;
       sb.Append(TestCommon.IntToString(len));
       return sb.ToString();
    }

    public static EDecimal GenerateEDecimalSmall(IRandomGenExtended wrapper) {
       if (wrapper == null) {
         throw new ArgumentNullException(nameof(wrapper));
       }
       if (wrapper.GetInt32(2) == 0) {
         EInteger eix = EInteger.FromBytes(
            RandomByteString(wrapper, 1 + wrapper.GetInt32(36)),
            true);
         int exp = wrapper.GetInt32(25) - 12;
         return EDecimal.Create(eix, exp);
       }
       return EDecimal.FromString(GenerateEDecimalSmallString(wrapper, false));
    }

    public static EDecimal RandomEDecimal(IRandomGenExtended r) {
       return RandomEDecimal(r, null);
    }

    public static EDecimal RandomEDecimal(IRandomGenExtended r, string[]
decimalString) {
      if (r == null) {
        throw new ArgumentNullException(nameof(r));
      }
      if (r.GetInt32(100) == 0) {
        int x = r.GetInt32(3);
        if (x == 0) {
          if (decimalString != null) {
            decimalString[0] = "Infinity";
          }
          return EDecimal.PositiveInfinity;
        }
        if (x == 1) {
          if (decimalString != null) {
            decimalString[0] = "-Infinity";
          }
          return EDecimal.NegativeInfinity;
        }
        if (x == 2) {
          if (decimalString != null) {
            decimalString[0] = "NaN";
          }
          return EDecimal.NaN;
        }
        // Signaling NaN currently not generated because
        // it doesn't round-trip as well
      }
      if (r.GetInt32(100) < 10) {
        string str = RandomDecimalString(r);
        if (str.Length < 500) {
          if (decimalString != null) {
            decimalString[0] = str;
          }
          return EDecimal.FromString(str);
        }
      }
      EInteger emant = RandomEInteger(r);
      EInteger eexp = RandomEInteger(r);
      EDecimal ed = EDecimal.Create(emant, eexp);
      if (decimalString != null) {
        decimalString[0] = emant.ToString() + "E" + eexp.ToString();
      }
      return ed;
    }

    public static EInteger RandomEInteger(IRandomGenExtended r) {
      if (r == null) {
        throw new ArgumentNullException(nameof(r));
      }
      int selection = r.GetInt32(100);
      if (selection < 10) {
        int count = r.GetInt32(MaxNumberLength);
        count = (int)(((long)count * r.GetInt32(MaxNumberLength)) /
MaxNumberLength);
        count = (int)(((long)count * r.GetInt32(MaxNumberLength)) /
MaxNumberLength);
        count = Math.Max(count, 1);
        byte[] bytes = RandomByteString(r, count);
        return EInteger.FromBytes(bytes, true);
      } else {
        byte[] bytes = RandomByteString(
          r,
          r.GetInt32(MaxShortNumberLength) + 1);
        return EInteger.FromBytes(bytes, true);
      }
    }

    public static EFloat RandomEFloat(IRandomGenExtended r) {
      if (r == null) {
        throw new ArgumentNullException(nameof(r));
      }
      if (r.GetInt32(100) == 0) {
        int x = r.GetInt32(3);
        if (x == 0) {
          return EFloat.PositiveInfinity;
        }
        if (x == 1) {
          return EFloat.NegativeInfinity;
        }
        if (x == 2) {
          return EFloat.NaN;
        }
      }
      return EFloat.Create(
          RandomEInteger(r),
          (EInteger)(r.GetInt32(400) - 200));
    }

    public static String RandomBigIntString(IRandomGenExtended r) {
      if (r == null) {
        throw new ArgumentNullException(nameof(r));
      }
      int count = r.GetInt32(MaxShortNumberLength) + 1;
      var sb = new StringBuilder();
      if (r.GetInt32(2) == 0) {
        sb.Append('-');
      }
      sb.Append((char)('1' + r.GetInt32(9)));
      --count;
      AppendRandomDecimals(r, sb, count);
      return sb.ToString();
    }

    public static EInteger RandomSmallIntegral(IRandomGenExtended r) {
      if (r == null) {
        throw new ArgumentNullException(nameof(r));
      }
      int count = r.GetInt32(MaxShortNumberLength / 2) + 1;
      var sb = new StringBuilder();
      if (r.GetInt32(2) == 0) {
        sb.Append('-');
      }
      sb.Append((char)('1' + r.GetInt32(9)));
      --count;
      AppendRandomDecimals(r, sb, count);
      return EInteger.FromString(sb.ToString());
    }

    public static String RandomDecimalString(IRandomGenExtended r) {
      return RandomDecimalString(r, false, true);
    }

    private static char[] charTable = {
      '0', '0', '0', '1', '1', '1', '2', '2', '2', '3', '3', '3', '4', '4', '4',
      '5', '5', '5', '6', '6', '6', '7', '7', '7', '8', '8', '8', '9', '9', '9',
    };

    // Special 10-digit-long strings
    private static string[] valueSpecialDecimals = {
      "1000000000",
      "0000000001",
      "4999999999",
      "5000000000",
      "5000000001",
      "5500000000",
      "0000000000",
      "9999999999",
    };

    // Special 40-digit-long strings
    private static string[] valueSpecialDecimals2 = {
      "1000000000000000000000000000000000000000",
      "0000000000000000000000000000000000000001",
      "4999999999999999999999999999999999999999",
      "5000000000000000000000000000000000000000",
      "5000000000000000000000000000000000000001",
      "5500000000000000000000000000000000000000",
      "0000000000000000000000000000000000000000",
      "9999999999999999999999999999999999999999",
    };

    private static void AppendRandomDecimalsLong(
      IRandomGenExtended r,
      StringBuilder sb,
      long count) {
      if (sb == null) {
        throw new ArgumentNullException(nameof(sb));
      }
      if (r == null) {
        throw new ArgumentNullException(nameof(r));
      }
      if (count > 0) {
        var buflen = (int)Math.Min(0x10000, Math.Max(count + 8, 64));
        var buffer = new byte[buflen];
        while (count > 0) {
          r.GetBytes(buffer, 0, buflen);
          var i = 0;
          while (i < buflen && count > 0) {
              int x = ((int)buffer[i]) & 31;
              if (x < 30) {
                sb.Append(charTable[x]);
                --count;
                ++i;
              } else if (count >= 40 && i + 1 < buflen) {
                int y = (((int)buffer[i + 1]) & 0xff) %
valueSpecialDecimals2.Length;
                sb.Append(valueSpecialDecimals2[y]);
                count -= 40;
                i += 2;
              } else if (count >= 10 && i + 1 < buflen) {
                int y = (((int)buffer[i + 1]) & 0xff) %
valueSpecialDecimals.Length;
                sb.Append(valueSpecialDecimals[y]);
                count -= 10;
                i += 2;
              } else {
                ++i;
              }
            }
        }
      }
    }

    private static void AppendRandomDecimals(
      IRandomGenExtended r,
      StringBuilder sb,
      int smallCount) {
      AppendRandomDecimalsLong(r, sb, smallCount);
    }

    public static String RandomDecimalString(
      IRandomGenExtended r,
      bool extended,
      bool limitedExponent) {
      if (r == null) {
        throw new ArgumentNullException(nameof(r));
      }
      if (r.GetInt32(100) < 95) {
        return GenerateEDecimalSmallString(r, extended);
      }
      long count = ((long)r.GetInt32(MaxNumberLength) *
r.GetInt32(MaxNumberLength)) / MaxNumberLength;
      count = ((long)count *
r.GetInt32(MaxNumberLength)) / MaxNumberLength;
      count = Math.Max(1, count);
      long afterPointCount = 0;
      long exponentCount = 0;
      var smallExponent = false;
      if (r.GetInt32(2) == 0) {
        afterPointCount = ((long)r.GetInt32(MaxNumberLength) *
r.GetInt32(MaxNumberLength)) / MaxNumberLength;
        afterPointCount = ((long)afterPointCount *
r.GetInt32(MaxNumberLength)) / MaxNumberLength;
        afterPointCount = Math.Max(1, afterPointCount);
      }
      if (r.GetInt32(2) == 0) {
        if (limitedExponent || r.GetInt32(10) > 0) {
          exponentCount = 5;
        } else {
          exponentCount = ((long)r.GetInt32(MaxNumberLength) *
r.GetInt32(MaxNumberLength)) / MaxNumberLength;
          exponentCount = ((long)exponentCount *
r.GetInt32(MaxNumberLength)) / MaxNumberLength;
          exponentCount = ((long)exponentCount *
r.GetInt32(MaxNumberLength)) / MaxNumberLength;
          exponentCount = Math.Max(1, exponentCount);
        }
      }
      var bufferSize = (int)Math.Min(
         Int32.MaxValue,
         8 + count + afterPointCount + exponentCount);
      var sb = new StringBuilder(bufferSize);
      if (r.GetInt32(2) == 0) {
        sb.Append('-');
      }
      if (!extended) {
        sb.Append((char)('1' + r.GetInt32(9)));
        --count;
      }
      AppendRandomDecimalsLong(r, sb, count);
      if (afterPointCount > 0) {
        sb.Append('.');
        AppendRandomDecimalsLong(r, sb, afterPointCount);
      }
      if (exponentCount > 0) {
        int rr = r.GetInt32(3);
        if (rr == 0) {
          sb.Append("E");
        } else if (rr == 1) {
   sb.Append("E+");
 } else if (rr == 2) {
   sb.Append("E-");
 }
        if (smallExponent) {
   sb.Append(TestCommon.IntToString(r.GetInt32(10000)));
        } else {
          AppendRandomDecimalsLong(r, sb, exponentCount);
        }
      }
      return sb.ToString();
    }
  }
}
