# Power Diary Interview

## author: Marcin Kern
### date: 23.09.2023

## Assumptions
- there's only one chat room

## Technical Decisions
- simplistic migration "by hand" to let the DB be reused between app starts
- data is being generated for up to 1-year back (from migration), which takes a while during first start