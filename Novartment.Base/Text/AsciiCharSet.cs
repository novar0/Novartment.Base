using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Набор символов ASCII.
	/// </summary>
	public static class AsciiCharSet
	{
		/// <summary>
		/// Максимально допустимый код символа в ASCII (127).
		/// </summary>
		public const char MaxCharValue = (char)127;

		/// <summary>
		/// Таблица принадлежности символов различным типам.
		/// </summary>
		public static readonly IReadOnlyList<short> Classes = new short[]
		{
/*   0x00 000 */ 0,
/*   0x01 001 */ 0,
/*   0x02 002 */ 0,
/*   0x03 003 */ 0,
/*   0x04 004 */ 0,
/*   0x05 005 */ 0,
/*   0x06 006 */ 0,
/*   0x07 007 */ 0,
/*   0x08 008 */ 0,
/*   0x09 009 */ (short)AsciiCharClasses.WhiteSpace,
/*   0x0a 010 */ 0,
/*   0x0b 011 */ 0,
/*   0x0c 012 */ 0,
/*   0x0d 013 */ 0,
/*   0x0e 014 */ 0,
/*   0x0f 015 */ 0,
/*   0x10 016 */ 0,
/*   0x11 017 */ 0,
/*   0x12 018 */ 0,
/*   0x13 019 */ 0,
/*   0x14 020 */ 0,
/*   0x15 021 */ 0,
/*   0x16 022 */ 0,
/*   0x17 023 */ 0,
/*   0x18 024 */ 0,
/*   0x19 025 */ 0,
/*   0x1a 026 */ 0,
/*   0x1b 027 */ 0,
/*   0x1c 028 */ 0,
/*   0x1d 029 */ 0,
/*   0x1e 030 */ 0,
/*   0x1f 031 */ 0,
/*   0x20 032 */ (short)(AsciiCharClasses.WhiteSpace | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured),
/* ! 0x21 033 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* " 0x22 034 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* # 0x23 035 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* $ 0x24 036 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* % 0x25 037 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* & 0x26 038 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* ' 0x27 039 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* ( 0x28 040 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* ) 0x29 041 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* * 0x2a 042 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* + 0x2b 043 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* , 0x2c 044 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* - 0x2d 045 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme),
/* . 0x2e 046 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Token | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme),
/* / 0x2f 047 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* 0 0x30 048 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 1 0x31 049 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 2 0x32 050 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 3 0x33 051 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 4 0x34 052 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 5 0x35 053 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 6 0x36 054 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 7 0x37 055 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 8 0x38 056 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 9 0x39 057 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* : 0x3a 058 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* ; 0x3b 059 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* < 0x3c 060 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* = 0x3d 061 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.Atom | AsciiCharClasses.UrlSchemePart),
/* > 0x3e 062 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* ? 0x3f 063 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.Atom | AsciiCharClasses.UrlSchemePart),
/* @ 0x40 064 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* A 0x41 065 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* B 0x42 066 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* C 0x43 067 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* D 0x44 068 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* E 0x45 069 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* F 0x46 070 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* G 0x47 071 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* H 0x48 072 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* I 0x49 073 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* J 0x4a 074 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* K 0x4b 075 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* L 0x4c 076 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* M 0x4d 077 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* N 0x4e 078 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* O 0x4f 079 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* P 0x50 080 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* Q 0x51 081 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* R 0x52 082 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* S 0x53 083 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* T 0x54 084 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* U 0x55 085 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* V 0x56 086 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* W 0x57 087 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* X 0x58 088 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* Y 0x59 089 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* Z 0x5a 090 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* [ 0x5b 091 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* \ 0x5c 092 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* ] 0x5d 093 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* ^ 0x5e 094 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* _ 0x5f 095 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* ` 0x60 096 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* a 0x61 097 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* b 0x62 098 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* c 0x63 099 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* d 0x64 100 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* e 0x65 101 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* f 0x66 102 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* g 0x67 103 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* h 0x68 104 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* i 0x69 105 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* j 0x6a 106 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* k 0x6b 107 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* l 0x6c 108 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* m 0x6d 109 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* n 0x6e 110 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* o 0x6f 111 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* p 0x70 112 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* q 0x71 113 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* r 0x72 114 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* s 0x73 115 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* t 0x74 116 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* u 0x75 117 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* v 0x76 118 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* w 0x77 119 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* x 0x78 120 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* y 0x79 121 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* z 0x7a 122 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* { 0x7b 123 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* | 0x7c 124 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* } 0x7d 125 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* ~ 0x7e 126 */ (short)(AsciiCharClasses.Visible | AsciiCharClasses.Domain | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/*   0x7f 127 */ 0,
		};

		/// <summary>
		/// Проверяет принадлежит ли указанный символ указанному типу.
		/// </summary>
		/// <param name="character">Проверяемый символ.</param>
		/// <param name="charClass">Тип символов, принадлеждность которому проверяется.</param>
		/// <returns>True если символ принадлежит указанному типу, иначе False.</returns>
		public static bool IsCharOfClass (char character, AsciiCharClasses charClass)
		{
			return (character < Classes.Count) && ((Classes[character] & (short)charClass) != 0);
		}

		/// <summary>
		/// Проверяет принадлежат ли все символы указанной строки указанному типу.
		/// </summary>
		/// <param name="value">Строка для проверки.</param>
		/// <param name="characterClass">Тип символов, приндлеждность которому проверяется.</param>
		/// <returns>True если все символы строки принадлежат указанному типу, иначе False.</returns>
		public static bool IsAllOfClass (string value, AsciiCharClasses characterClass)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			return IsAllOfClass (value.AsSpan (), characterClass);
		}

		/// <summary>
		/// Проверяет принадлежат ли все символы указанной части строки указанному типу.
		/// </summary>
		/// <param name="value">Строка для проверки.</param>
		/// <param name="characterClass">Тип символов, приндлеждность которому проверяется.</param>
		/// <returns>True если все символы строки принадлежат указанному типу, иначе False.</returns>
		public static bool IsAllOfClass (ReadOnlySpan<char> value, AsciiCharClasses characterClass)
		{
			if (characterClass == AsciiCharClasses.None)
			{
				throw new ArgumentOutOfRangeException (nameof (characterClass));
			}

			Contract.EndContractBlock ();

			var currentPos = 0;
			while (currentPos < value.Length)
			{
				var character = value[currentPos];
				if ((character >= Classes.Count) || ((Classes[character] & (short)characterClass) == 0))
				{
					return false;
				}

				currentPos++;
			}

			return true;
		}

		/// <summary>
		/// Проверяет встречается ли среди символов указанной строки символы указанного типа.
		/// </summary>
		/// <param name="value">Строка для проверки.</param>
		/// <param name="characterClass">Тип символов, приндлеждность которому проверяется.</param>
		/// <returns>True если среди символов строки встречаются символы указанного типа, иначе False.</returns>
		public static bool IsAnyOfClass (string value, AsciiCharClasses characterClass)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			if (characterClass == AsciiCharClasses.None)
			{
				throw new ArgumentOutOfRangeException (nameof (characterClass));
			}

			Contract.EndContractBlock ();

			return IsAnyOfClass (value.AsSpan (), characterClass);
		}

		/// <summary>
		/// Проверяет встречается ли среди символов указанной строки символы указанного типа.
		/// </summary>
		/// <param name="value">Строка для проверки.</param>
		/// <param name="characterClass">Тип символов, приндлеждность которому проверяется.</param>
		/// <returns>True если среди символов строки встречаются символы указанного типа, иначе False.</returns>
		public static bool IsAnyOfClass (ReadOnlySpan<char> value, AsciiCharClasses characterClass)
		{
			if (characterClass == AsciiCharClasses.None)
			{
				throw new ArgumentOutOfRangeException (nameof (characterClass));
			}

			Contract.EndContractBlock ();

			int currentPos = 0;
			while (currentPos < value.Length)
			{
				var isSurrogatePair = char.IsSurrogatePair (value[currentPos], value[currentPos + 1]);
				if (isSurrogatePair)
				{
					currentPos += 2;
				}
				else
				{
					var character = value[currentPos];
					if ((character < Classes.Count) && ((Classes[character] & (short)characterClass) != 0))
					{
						return true;
					}

					currentPos++;
				}
			}

			return false;
		}

		/// <summary>
		/// Создаёт в указанном массиве байт представление указанного сегмента массива ASCII-символов.
		/// </summary>
		/// <param name="value">Массив ASCII-символов, для которого требуется получить байтовое представление.</param>
		/// <param name="buffer">Массив байтов, в который будет помещена результирующая последовательность байтов.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">Происходит если размер value больше, чем buffer.</exception>
		/// <exception cref="System.FormatException">Происходит когда во входных данных встречается символ, не входящий в набор ASCII.</exception>
		/// <remarks>Семантический аналог ASCIIEncoding.GetBytes (), но более быстрый.</remarks>
		public static void GetBytes (ReadOnlySpan<char> value, Span<byte> buffer)
		{
			if (value.Length > buffer.Length)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

			Contract.EndContractBlock ();

			for (var idx = 0; idx < value.Length; idx++)
			{
				var ch = value[idx];
				if (ch > MaxCharValue)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Invalid ASCII char U+{ch:x}. Acceptable range is from U+0000 to U+007F."));
				}

				buffer[idx] = (byte)ch;
			}
		}

		/// <summary>
		/// Создаёт ASCII-строковое представление указанного сегмента массива байтов.
		/// </summary>
		/// <param name="value">Массив байт, указанный сегмент которого будет преобразован в ASCII-строку.</param>
		/// <returns>Массив ASCII-символов, созданный из указанного сегмента массива байтов.</returns>
		/// <exception cref="System.FormatException">Происходит когда во входных данных встречается символ, не входящий в набор ASCII.</exception>
		/// <remarks>Семантический аналог ASCIIEncoding.GetChars (), но более быстрый.</remarks>
		public static char[] GetChars (ReadOnlySpan<byte> value)
		{
			var result = new char[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				var b = value[i];
				if (b > MaxCharValue)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Invalid ASCII char U+{b:x}. Acceptable range is from U+0000 to U+007F."));
				}

				result[i] = (char)b;
			}

			return result;
		}

		/// <summary>
		/// Создаёт ASCII-строковое представление указанного сегмента массива байтов.
		/// </summary>
		/// <param name="value">Массив байт, указанный сегмент которого будет преобразован в ASCII-строку.</param>
		/// <returns>ASCII-строка, созданная из указанного сегмента массива байтов.</returns>
		/// <exception cref="System.FormatException">Происходит когда во входных данных встречается символ, не входящий в набор ASCII.</exception>
		/// <remarks>Семантический аналог ASCIIEncoding.GetString (), но более быстрый.</remarks>
		public static string GetString (ReadOnlySpan<byte> value)
		{
			return GetStringInternal (value, char.MaxValue);
		}

		/// <summary>
		/// Создаёт ASCII-строковое представление указанного сегмента массива байтов,
		/// заменяя не-ASCII символы указанным символом-заменителем.
		/// </summary>
		/// <param name="value">Массив байт, указанный сегмент которого будет преобразован в ASCII-строку.</param>
		/// <param name="substituteCharacter">Символ-заменитель, который будет вставлен вместо не-ASCII символов.</param>
		/// <returns>ASCII-строка, созданная из указанного сегмента массива байтов.</returns>
		/// <remarks>Семантический аналог ASCIIEncoding.GetString (), но более быстрый.</remarks>
		public static string GetStringMaskingInvalidChars (ReadOnlySpan<byte> value, char substituteCharacter)
		{
			return GetStringInternal (value, substituteCharacter);
		}

		/// <summary>
		/// Преобразует указанную строку в вид 'quoted-string'.
		/// В результирующей строке в начале и конце будут вставлены кавычки,
		/// а перед кавычками и знаками косой черты в исходной строке будут вставлены дополнительные знаки косой черты.
		/// </summary>
		/// <param name="text">Значение, которое надо представить в виде 'quoted-string'.</param>
		/// <returns>Указанное значение, преобразованное в вид 'quoted-string'.</returns>
		/// <exception cref="System.ArgumentNullException">Происходит если text равен null.</exception>
		/// <exception cref="System.FormatException">Происходит когда во входных данных встречается символ, не входящий в набор ASCII.</exception>
		public static string Quote (string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			if (text.Length < 1)
			{
				return "\"\"";
			}

			var result = new StringBuilder ("\"", text.Length + 2); // начальная кавычка и мин. размер который точно понадобится
			for (var srcPos = 0; srcPos < text.Length; srcPos++)
			{
				var character = text[srcPos];
				var isVisibleOrWS = AsciiCharSet.IsCharOfClass (character, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
				if (!isVisibleOrWS)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Value contains invalid for 'quoted-sting' character U+{character:x}. Expected characters are U+0009 and U+0020...U+007E."));
				}

				if ((character == '\"') || (character == '\\'))
				{
					// кавычка либо косая черта предваряется косой чертой
					result.Append ('\\');
				}

				result.Append (character);
			}

			result.Append ('"'); // конечная кавычка
			return result.ToString ();
		}

		/// <summary>
		/// Преобразует указанную строку в вид 'quoted-string'.
		/// В результирующей строке в начале и конце будут вставлены кавычки,
		/// а перед кавычками и знаками косой черты в исходной строке будут вставлены дополнительные знаки косой черты.
		/// </summary>
		/// <param name="text">Значение, которое надо представить в виде 'quoted-string'.</param>
		/// <param name="buf">Буфер, куда будет записано значение, преобразованное в вид 'quoted-string'.</param>
		/// <returns>Количество знаков, записанных в буфер.</returns>
		/// <exception cref="System.FormatException">Происходит когда во входных данных встречается символ, не входящий в набор ASCII.</exception>
		public static int Quote (ReadOnlySpan<char> text, Span<char> buf)
		{
			var outPos = 0;
			buf[outPos++] = '"'; // начальная кавычка
			for (var srcPos = 0; srcPos < text.Length; srcPos++)
			{
				var character = text[srcPos];
				var isVisibleOrWS = AsciiCharSet.IsCharOfClass (character, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
				if (!isVisibleOrWS)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Value contains invalid for 'quoted-sting' character U+{character:x}. Expected characters are U+0009 and U+0020...U+007E."));
				}

				if ((character == '\"') || (character == '\\'))
				{
					// кавычка либо косая черта предваряется косой чертой
					buf[outPos++] = '\\';
				}

				buf[outPos++] = character;
			}

			buf[outPos++] = '"'; // конечная кавычка
			return outPos;
		}

		/// <summary>
		/// Преобразует указанную строку в вид 'quoted-string'.
		/// В результирующей строке в начале и конце будут вставлены кавычки,
		/// а перед кавычками и знаками косой черты в исходной строке будут вставлены дополнительные знаки косой черты.
		/// </summary>
		/// <param name="text">Значение, которое надо представить в виде 'quoted-string'.</param>
		/// <param name="buf">Буфер, куда будет записано значение, преобразованное в вид 'quoted-string'.</param>
		/// <returns>Количество знаков, записанных в буфер.</returns>
		/// <exception cref="System.FormatException">Происходит когда во входных данных встречается символ, не входящий в набор ASCII.</exception>
		public static int QuoteToUtf8 (ReadOnlySpan<char> text, Span<byte> buf)
		{
			var outPos = 0;
			buf[outPos++] = (byte)'"'; // начальная кавычка
			for (var srcPos = 0; srcPos < text.Length; srcPos++)
			{
				var character = text[srcPos];
				var isVisibleOrWS = AsciiCharSet.IsCharOfClass (character, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
				if (!isVisibleOrWS)
				{
					throw new FormatException (FormattableString.Invariant (
						$"Value contains invalid for 'quoted-sting' character U+{character:x}. Expected characters are U+0009 and U+0020...U+007E."));
				}

				if ((character == '\"') || (character == '\\'))
				{
					// кавычка либо косая черта предваряется косой чертой
					buf[outPos++] = (byte)'\\';
				}

				buf[outPos++] = (byte)character;
			}

			buf[outPos++] = (byte)'"'; // конечная кавычка
			return outPos;
		}

		/// <summary>
		/// Проверяет, является ли указанная строка корректным доменным именем в Интернете.
		/// </summary>
		/// <param name="name">Строка для проверки соответсвия формату доменного имени Интернета.</param>
		/// <returns>True если указанная строка является корректным доменным именем в Интернете, иначе False.</returns>
		/// <remarks>Также соответствует 'dot-atom' RFC 5322.</remarks>
		public static bool IsValidInternetDomainName (string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException (nameof (name));
			}

			Contract.EndContractBlock ();

			if (name.Length < 1)
			{
				return false;
			}

			int lastDotPosition = -1;
			int position;
			for (position = 0; position < name.Length; position++)
			{
				var c = name[position];
				if (c == '.')
				{
					if ((position - lastDotPosition) < 2)
					{
						// от точки до точки должен быть мин. один символ, т.е. расстояние >= 2
						return false;
					}

					lastDotPosition = position;
				}
				else
				{
					var isCharValid = AsciiCharSet.IsCharOfClass (c, AsciiCharClasses.Atom);
					if (!isCharValid)
					{
						return false;
					}
				}
			}

			if ((lastDotPosition >= 0) && ((position - lastDotPosition) < 2))
			{
				return false;
			}

			return true;
		}

		private static string GetStringInternal (ReadOnlySpan<byte> value, char substituteChar)
		{
			if (value.Length < 1)
			{
				return string.Empty;
			}

			var result = new char[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				var b = value[i];
				if (b > MaxCharValue)
				{
					if (substituteChar == char.MaxValue)
					{
						throw new FormatException (FormattableString.Invariant (
							$"Invalid ASCII char U+{b:x}. Acceptable range is from U+0000 to U+007F."));
					}

					result[i] = substituteChar;
				}
				else
				{
					result[i] = (char)b;
				}
			}

			return new string (result);
		}
	}
}
