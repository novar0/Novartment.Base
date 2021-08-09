using System;
using System.Text;

namespace Novartment.Base.Text
{
	/// <summary>
	/// The ASCII character set.
	/// </summary>
	public static class AsciiCharSet
	{
		/// <summary>
		/// Represents the largest possible value of ASCII character.
		/// </summary>
		public static readonly char MaxCharValue = (char)127;

		/// <summary>
		/// Table of values belonging to different types.
		/// The index in the table corresponds to the character code.
		/// </summary>
		public static readonly ReadOnlyMemory<AsciiCharClasses> ValueClasses = new AsciiCharClasses[]
		{
/*   0x00 000 */ AsciiCharClasses.None,
/*   0x01 001 */ AsciiCharClasses.None,
/*   0x02 002 */ AsciiCharClasses.None,
/*   0x03 003 */ AsciiCharClasses.None,
/*   0x04 004 */ AsciiCharClasses.None,
/*   0x05 005 */ AsciiCharClasses.None,
/*   0x06 006 */ AsciiCharClasses.None,
/*   0x07 007 */ AsciiCharClasses.None,
/*   0x08 008 */ AsciiCharClasses.None,
/*   0x09 009 */ AsciiCharClasses.WhiteSpace,
/*   0x0a 010 */ AsciiCharClasses.None,
/*   0x0b 011 */ AsciiCharClasses.None,
/*   0x0c 012 */ AsciiCharClasses.None,
/*   0x0d 013 */ AsciiCharClasses.None,
/*   0x0e 014 */ AsciiCharClasses.None,
/*   0x0f 015 */ AsciiCharClasses.None,
/*   0x10 016 */ AsciiCharClasses.None,
/*   0x11 017 */ AsciiCharClasses.None,
/*   0x12 018 */ AsciiCharClasses.None,
/*   0x13 019 */ AsciiCharClasses.None,
/*   0x14 020 */ AsciiCharClasses.None,
/*   0x15 021 */ AsciiCharClasses.None,
/*   0x16 022 */ AsciiCharClasses.None,
/*   0x17 023 */ AsciiCharClasses.None,
/*   0x18 024 */ AsciiCharClasses.None,
/*   0x19 025 */ AsciiCharClasses.None,
/*   0x1a 026 */ AsciiCharClasses.None,
/*   0x1b 027 */ AsciiCharClasses.None,
/*   0x1c 028 */ AsciiCharClasses.None,
/*   0x1d 029 */ AsciiCharClasses.None,
/*   0x1e 030 */ AsciiCharClasses.None,
/*   0x1f 031 */ AsciiCharClasses.None,
/*   0x20 032 */ (AsciiCharClasses.WhiteSpace | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured),
/* ! 0x21 033 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* " 0x22 034 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* # 0x23 035 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* $ 0x24 036 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* % 0x25 037 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* & 0x26 038 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* ' 0x27 039 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* ( 0x28 040 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* ) 0x29 041 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* * 0x2a 042 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* + 0x2b 043 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* , 0x2c 044 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* - 0x2d 045 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme),
/* . 0x2e 046 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Token | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme),
/* / 0x2f 047 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Atom | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* 0 0x30 048 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 1 0x31 049 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 2 0x32 050 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 3 0x33 051 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 4 0x34 052 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 5 0x35 053 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 6 0x36 054 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 7 0x37 055 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 8 0x38 056 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* 9 0x39 057 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Digit | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* : 0x3a 058 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* ; 0x3b 059 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* < 0x3c 060 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* = 0x3d 061 */ (AsciiCharClasses.Visible | AsciiCharClasses.Atom | AsciiCharClasses.UrlSchemePart),
/* > 0x3e 062 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* ? 0x3f 063 */ (AsciiCharClasses.Visible | AsciiCharClasses.Atom | AsciiCharClasses.UrlSchemePart),
/* @ 0x40 064 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.UrlSchemePart),
/* A 0x41 065 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* B 0x42 066 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* C 0x43 067 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* D 0x44 068 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* E 0x45 069 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* F 0x46 070 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* G 0x47 071 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* H 0x48 072 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* I 0x49 073 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* J 0x4a 074 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* K 0x4b 075 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* L 0x4c 076 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* M 0x4d 077 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* N 0x4e 078 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* O 0x4f 079 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* P 0x50 080 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* Q 0x51 081 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* R 0x52 082 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* S 0x53 083 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* T 0x54 084 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* U 0x55 085 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* V 0x56 086 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* W 0x57 087 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* X 0x58 088 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* Y 0x59 089 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* Z 0x5a 090 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.Base64Alphabet),
/* [ 0x5b 091 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* \ 0x5c 092 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* ] 0x5d 093 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured),
/* ^ 0x5e 094 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* _ 0x5f 095 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/* ` 0x60 096 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* a 0x61 097 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* b 0x62 098 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* c 0x63 099 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* d 0x64 100 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* e 0x65 101 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* f 0x66 102 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* g 0x67 103 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* h 0x68 104 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* i 0x69 105 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* j 0x6a 106 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* k 0x6b 107 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* l 0x6c 108 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* m 0x6d 109 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* n 0x6e 110 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* o 0x6f 111 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* p 0x70 112 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* q 0x71 113 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* r 0x72 114 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* s 0x73 115 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* t 0x74 116 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* u 0x75 117 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* v 0x76 118 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* w 0x77 119 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* x 0x78 120 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* y 0x79 121 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* z 0x7a 122 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.QEncodingAllowedInStructured | AsciiCharClasses.Alpha | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart | AsciiCharClasses.UrlScheme | AsciiCharClasses.Base64Alphabet),
/* { 0x7b 123 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* | 0x7c 124 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* } 0x7d 125 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken),
/* ~ 0x7e 126 */ (AsciiCharClasses.Visible | AsciiCharClasses.QEncodingAllowedInUnstructured | AsciiCharClasses.Atom | AsciiCharClasses.Token | AsciiCharClasses.ExtendedToken | AsciiCharClasses.UrlSchemePart),
/*   0x7f 127 */ AsciiCharClasses.None,
		};

		/// <summary>
		/// Checks whether all characters in the specified string belong to the specified class or combination of classes.
		/// </summary>
		/// <param name="value">The string to check.</param>
		/// <param name="characterClass">The combination of classes of characters to be checked for.</param>
		/// <returns>True if all characters in the string belong to the specified class; otherwise False.</returns>
		public static bool IsAllOfClass (string value, AsciiCharClasses characterClass)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			return IsAllOfClass (value.AsSpan (), characterClass);
		}

		/// <summary>
		/// Checks whether all characters in the specified string belong to the specified class or combination of classes.
		/// </summary>
		/// <param name="value">The string to check.</param>
		/// <param name="characterClass">The combination of classes of characters to be checked for.</param>
		/// <returns>True if all characters in the string belong to the specified class; otherwise False.</returns>
		public static bool IsAllOfClass (ReadOnlySpan<char> value, AsciiCharClasses characterClass)
		{
			if (characterClass == AsciiCharClasses.None)
			{
				throw new ArgumentOutOfRangeException (nameof (characterClass));
			}

			var currentPos = 0;
			var classes = ValueClasses.Span;
			while (currentPos < value.Length)
			{
				// суррогатные пары можно игнорировать, их части не попадают ни в какой класс ASCII
				var character = value[currentPos];
				if ((character >= classes.Length) || ((classes[character] & characterClass) == 0))
				{
					return false;
				}

				currentPos++;
			}

			return true;
		}

		/// <summary>
		/// Checks whether the specified string contains characters of the specified class or combination of classes.
		/// </summary>
		/// <param name="value">The string to check.</param>
		/// <param name="characterClass">The combination of classes of characters to be checked for.</param>
		/// <returns>True if the string contains characters of the specified class; otherwise False.</returns>
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

			return IsAnyOfClass (value.AsSpan (), characterClass);
		}

		/// <summary>
		/// Checks whether the specified string contains characters of the specified class or combination of classes.
		/// </summary>
		/// <param name="value">The string to check.</param>
		/// <param name="characterClass">The combination of classes of characters to be checked for.</param>
		/// <returns>True if the string contains characters of the specified class; otherwise False.</returns>
		public static bool IsAnyOfClass (ReadOnlySpan<char> value, AsciiCharClasses characterClass)
		{
			if (characterClass == AsciiCharClasses.None)
			{
				throw new ArgumentOutOfRangeException (nameof (characterClass));
			}

			int currentPos = 0;
			var asciiClasses = ValueClasses.Span;
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
					if ((character < asciiClasses.Length) && ((asciiClasses[character] & characterClass) != 0))
					{
						return true;
					}

					currentPos++;
				}
			}

			return false;
		}

		/// <summary>
		/// Creates a representation of the specified ASCII string in the specified region of memory.
		/// </summary>
		/// <param name="value">The ASCII string to encode.</param>
		/// <param name="buffer">The region of memory to which the resulting sequence of bytes will be written.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">The length of the value is greater than the buffer.</exception>
		/// <exception cref="System.FormatException">The input data contains a character that is not included in the ASCII character set.</exception>
		/// <remarks>A semantic analogue of ASCIIEncoding.GetBytes (), but faster.</remarks>
		public static void GetBytes (ReadOnlySpan<char> value, Span<byte> buffer)
		{
			if (value.Length > buffer.Length)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

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
		/// Creates an ASCII string representation of the specified region of memory.
		/// </summary>
		/// <param name="value">The region of memory that will be decoded to an ASCII string.</param>
		/// <returns>The ASCII string created from the specified region of memory.</returns>
		/// <exception cref="System.FormatException">The input data contains a character that is not included in the ASCII character set.</exception>
		/// <remarks>A semantic analogue of ASCIIEncoding.GetString (), but faster.</remarks>
		public static string GetString (ReadOnlySpan<byte> value)
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
					throw new FormatException (FormattableString.Invariant (
						$"Invalid ASCII char U+{b:x}. Acceptable range is from U+0000 to U+007F."));
				}

				result[i] = (char)b;
			}

			return new string (result);
		}

		/// <summary>
		/// Converts the specified string to the 'quoted-string' form.
		/// In the resulting string, quotes will be inserted at the beginning and end,
		/// and additional back slashes will be inserted before the quotes and slashes in the source string.
		/// </summary>
		/// <param name="text">The string to be converted in the 'quoted-string' form.</param>
		/// <returns>The specified string converted to the 'quoted-string' form.</returns>
		/// <exception cref="System.ArgumentNullException">The text is null.</exception>
		/// <exception cref="System.FormatException">The input data contains a character that is not included in the ASCII character set.</exception>
		public static string Quote (string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			if (text.Length < 1)
			{
				return "\"\"";
			}

			var result = new StringBuilder ("\"", text.Length + 2); // начальная кавычка и мин. размер который точно понадобится
			var charClasses = ValueClasses.Span;
			var allowedCharClasses = AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace;
			for (var srcPos = 0; srcPos < text.Length; srcPos++)
			{
				var character = text[srcPos];
				var isVisibleOrWS = (character < charClasses.Length) && ((charClasses[character] & allowedCharClasses) != 0);
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
		/// Converts the specified string to the 'quoted-string' form.
		/// In the resulting string, quotes will be inserted at the beginning and end,
		/// and additional back slashes will be inserted before the quotes and slashes in the source string.
		/// </summary>
		/// <param name="text">The string to be converted in the 'quoted-string' form.</param>
		/// <param name="buf">The buffer, where the specified string converted to the 'quoted-string' form will be written.</param>
		/// <returns>The number of characters written to the buffer.</returns>
		/// <exception cref="System.FormatException">The input data contains a character that is not included in the ASCII character set.</exception>
		public static int Quote (ReadOnlySpan<char> text, Span<char> buf)
		{
			if (buf.Length < (text.Length + 2))
			{
				throw new ArgumentOutOfRangeException (nameof (buf));
			}

			var outPos = 0;
			buf[outPos++] = '"'; // начальная кавычка
			var charClasses = ValueClasses.Span;
			var allowedCharClasses = AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace;
			for (var srcPos = 0; srcPos < text.Length; srcPos++)
			{
				var character = text[srcPos];
				var isVisibleOrWS = (character < charClasses.Length) && ((charClasses[character] & allowedCharClasses) != 0);
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
		/// Checks if the specified string is a valid Internet domain name.
		/// </summary>
		/// <param name="name">A string to check compliance with the Internet domain name format.</param>
		/// <returns>True if the specified string is a valid Internet domain name; otherwise False.</returns>
		/// <remarks>Also matches 'dot-atom' specification in the RFC 5322.</remarks>
		public static bool IsValidInternetDomainName (string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException (nameof (name));
			}

			if (name.Length < 1)
			{
				return false;
			}

			int lastDotPosition = -1;
			int position;
			var charClasses = ValueClasses.Span;
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
					var isCharValid = (c < charClasses.Length) && ((charClasses[c] & AsciiCharClasses.Atom) == AsciiCharClasses.Atom);
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
	}
}
