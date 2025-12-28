# Dutch Parser Pattern Matching Order - Critical!

## The Problem

**Input:** "vijf na half drie"  
**Expected:** 2:35 (5 minutes after 2:30)  
**Was Getting:** 2:30 (just "half drie")  

## Root Cause

The parser was checking patterns in the **wrong order**:

### ? Old Order (WRONG)
1. `TryParseKwartOver` - "kwart over [uur]"
2. `TryParseKwartVoor` - "kwart voor [uur]"
3. **`TryParseHalf`** - **"half [uur]"** ? This matched first!
4. `TryParseMinutenVoorHalf` - "[minuten] voor half [uur]"
5. `TryParseMinutenOverHalf` - "[minuten] over/na half [uur]"
6. ... other patterns

**What happened:**
- Input: "vijf na half drie"
- Pattern 3 (`half [uur]`) matched **"half drie"** = 2:30
- The "vijf na" part was ignored!
- Parser returned 2:30 instead of 2:35

## The Fix

### ? New Order (CORRECT)
1. **`TryParseMinutenVoorHalf`** - **"[minuten] voor half [uur]"** ? Check compound patterns FIRST!
2. **`TryParseMinutenOverHalf`** - **"[minuten] over/na half [uur]"** ? Check compound patterns FIRST!
3. `TryParseKwartOver` - "kwart over [uur]"
4. `TryParseKwartVoor` - "kwart voor [uur]"
5. `TryParseHalf` - "half [uur]" ? Now checked AFTER compound patterns
6. ... other patterns

**Now:**
- Input: "vijf na half drie"
- Pattern 2 (`[minuten] over/na half [uur]`) matches **"vijf na half drie"** = 2:35 ?
- Correct result!

## Parsing Rule

**ALWAYS check MORE SPECIFIC patterns before LESS SPECIFIC patterns!**

### Specificity Order (Most to Least):
1. **Compound patterns** (3+ words): "vijf na half drie"
2. **Simple patterns** (2 words): "half drie", "kwart over vijf"
3. **Single patterns** (1 word + modifier): "drie uur"

## Why This Matters

In natural language parsing, **substring matching can be dangerous**:
- "half drie" is a substring of "vijf na half drie"
- If you check "half drie" first, it matches and stops
- The more specific pattern never gets a chance

## Test Cases Affected

```csharp
"vijf na half drie" ? 2:35 ? (not 2:30)
"vijf over half twee" ? 1:35 ? (not 1:30)
"tien voor half acht" ? 7:20 ? (not 7:30)
```

## Lesson Learned

**Pattern matching order is CRITICAL in natural language parsers!**
- Always check compound/complex patterns first
- Then check simpler patterns
- Document the order with comments explaining why
