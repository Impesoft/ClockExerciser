# Dutch Time Parser - Consistent Dutch Naming

## Naming Convention
All method names, variable names, and regex patterns in `DutchTimeParser` now use **consistent Dutch naming** to match the language being parsed.

## Method Names (All in Dutch)

### Pattern Parsing Methods
- `TryParseKwartOver()` - Parses "kwart over [uur]"
- `TryParseKwartVoor()` - Parses "kwart voor [uur]"
- `TryParseHalf()` - Parses "half [uur]"
- `TryParseMinutenVoorHalf()` - Parses "[minuten] voor half [uur]"
- `TryParseMinutenOverHalf()` - Parses "[minuten] over/na half [uur]"
- `TryParseMinutenOver()` - Parses "[minuten] over/na [uur]"
- `TryParseMinutenVoor()` - Parses "[minuten] voor [uur]"
- `TryParseExactUur()` - Parses "[uur] uur"

### Helper Methods
- `TryParseMinuutWoord()` - Parses minute word/number
- `TryParseUurWoord()` - Parses hour word/number

### Regex Methods
- `KwartOverRegex()`
- `KwartVoorRegex()`
- `HalfRegex()`
- `MinutenVoorHalfRegex()`
- `MinutenOverHalfRegex()`
- `MinutenOverRegex()`
- `MinutenVoorRegex()`
- `ExactUurRegex()`

## Variable Names (All in Dutch)

### In parsing methods:
- `uur` - hour
- `minuten` - minutes
- `actueleUur` - actual hour (after calculation)
- `actueleMinuten` - actual minutes (after calculation)
- `woord` - word
- `uurStr` / `minutenStr` - hour/minute string from regex match

## Dutch-English Translation Reference

| Dutch | English | Usage |
|-------|---------|-------|
| `uur` | hour | Time component |
| `minuten` | minutes | Time component |
| `minuut` | minute | Single minute |
| `woord` | word | Text to parse |
| `kwart` | quarter | 15 minutes |
| `half` | half | 30 minutes |
| `over` | past/after | Direction indicator |
| `voor` | to/before | Direction indicator |
| `na` | after | Alternative to "over" |
| `actuele` | actual | Calculated value |

## Benefits of Consistent Dutch Naming

1. **Clarity**: Dutch names in Dutch parser - English names in English parser
2. **Maintainability**: Easy to understand context when reading code
3. **Cultural Accuracy**: Respects the language being parsed
4. **No Confusion**: Clear separation between parser implementations
5. **Self-Documenting**: Method names directly correspond to Dutch phrases

## Example Usage in Comments

```csharp
// "5 voor half twaalf" = 11:25 (5 minuten voor 11:30)
var actueleUur = uur == 1 ? 12 : uur - 1;
var actueleMinuten = 30 - minuten;
```

This makes it immediately clear we're working with Dutch time expressions!
