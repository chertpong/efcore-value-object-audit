# ValueObjectAuditing

Related to stackoverflow discussion: https://stackoverflow.com/questions/60946496/ef-core-how-to-audit-trail-with-value-objects

Cloned from https://github.com/ovirta/ValueObjectAuditing

If the whole app has one entity and one value object, we can get the old value(s) of Value Object from DELETED EntityEntry.

Checkout branch name `main` to see once the Value Object type is being referenced by more than one entity, EF Core will no longer produce DELETED EntityEntry.