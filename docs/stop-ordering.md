# Stop ordering

Stops are displayed by SortOrder, then Id. Dragging within a day or across days calls
POST /Trip/MoveStop with an antiforgery token. The endpoint checks ownership and
requires both days to belong to the same trip. A failed request restores the local
layout and asks the user to refresh to reconcile any uncertain network outcome.

Migration: 20260918120000_AddStopSortOrder. Apply it before deploying this model to
another database. It adds an integer column and initializes existing ordering from Id.

## Existing configured database

The new column and its migration-history entry were applied directly on 2026-09-18.
The database already had the renamed columns from RenameDayToDayNumber, but did
not record that old migration. Its Trips-to-users foreign key is absent, and the
orphan check found records blocking the full historical migration. Those records
were not changed. The two day/stop constraints removed by the attempted historical
migration were restored with their original names.

Do not blindly rerun `dotnet ef database update` on this database: first reconcile
its historical migration and orphaned records. The stop-ordering change is already
applied. Desktop HTML drag-and-drop is implemented; touch dragging has not been added.
