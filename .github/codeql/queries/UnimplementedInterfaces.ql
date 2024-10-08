/**
 * @name Unimplemented Interface
 * @description Finds interfaces that do not have any implementations.
 * @kind problem
 * @problem.severity error
 * @id csharp/unimplemented-interface
 * @tags clean-code
 */

import csharp

/**
 * Predicate to find interfaces without any implementing types.
 */
predicate isUnimplementedInterface(InterfaceType i) {
  not exists(Type t | t.implements(i))
}

from InterfaceType i
where isUnimplementedInterface(i)
select i, "Interface " + i.getName() + " does not have any implementations."
