# ValueObjectAuditing

Related to stackoverflow discussion: https://stackoverflow.com/questions/60946496/ef-core-how-to-audit-trail-with-value-objects

Cloned from https://github.com/ovirta/ValueObjectAuditing


After added another entity which has same value object data type, EF Core will no longer produce DELETED entityEntry.

see branch name `original` to see original version where the whole app contains only one entitty and one value object, the EF core will produce DELETED entityEntry if anyone updated that value object.


Damn... it's a bug
https://github.com/dotnet/efcore/issues/23418
