---
title: YAML Syntax
source: http://docs.ansible.com/ansible/YAMLSyntax.html
tags: [YAML,repost]
date: 2016-06-08
---

> source: http://docs.ansible.com/ansible/YAMLSyntax.html

This page provides a basic overview of correct YAML syntax, which is how Ansible playbooks (our configuration management language) are expressed.

We use YAML because it is easier for humans to read and write than other common data formats like XML or JSON. Further, there are libraries available in most programming languages for working with YAML.

You may also wish to read Playbooks at the same time to see how this is used in practice.

<!--more-->

## YAML Basics
For Ansible, nearly every YAML file starts with a list. Each item in the list is a list of key/value pairs, commonly called a “hash” or a “dictionary”. So, we need to know how to write lists and dictionaries in YAML.

There’s another small quirk to YAML. All YAML files (regardless of their association with Ansible or not) can optionally begin with --- and end with *....* This is part of the YAML format and indicates the start and end of a document.

All members of a list are lines beginning at the same indentation level starting with a "- " (a dash and a space):

```yaml
---
# A list of tasty fruits
fruits:
    - Apple
    - Orange
    - Strawberry
    - Mango
...
```

A dictionary is represented in a simple key: value form (the colon must be followed by a space):

```yaml
# An employee record
martin:
    name: Martin D'vloper
    job: Developer
    skill: Elite
```

More complicated data structures are possible, such as lists of dictionaries, dictionaries whose values are lists or a mix of both:

```yaml
# Employee records
-  martin:
    name: Martin D'vloper
    job: Developer
    skills:
      - python
      - perl
      - pascal
-  tabitha:
    name: Tabitha Bitumen
    job: Developer
    skills:
      - lisp
      - fortran
      - erlang
```

Dictionaries and lists can also be represented in an abbreviated form if you really want to:

```yaml
---
martin: {name: Martin D'vloper, job: Developer, skill: Elite}
fruits: ['Apple', 'Orange', 'Strawberry', 'Mango']
```

Ansible doesn’t really use these too much, but you can also specify a boolean value (true/false) in several forms:

```yaml
create_key: yes
needs_agent: no
knows_oop: True
likes_emacs: TRUE
uses_cvs: false
```

Values can span multiple lines using | or >. Spanning multiple lines using a | will include the newlines. Using a > will ignore newlines; it’s used to make what would otherwise be a very long line easier to read and edit. In either case the indentation will be ignored. Examples are:

```yaml
include_newlines: |
            exactly as you see
            will appear these three
            lines of poetry

ignore_newlines: >
            this is really a
            single line of text
            despite appearances
```

Let’s combine what we learned so far in an arbitrary YAML example. This really has nothing to do with Ansible, but will give you a feel for the format:

```yaml
---
# An employee record
name: Martin D'vloper
job: Developer
skill: Elite
employed: True
foods:
    - Apple
    - Orange
    - Strawberry
    - Mango
languages:
    perl: Elite
    python: Elite
    pascal: Lame
education: |
    4 GCSEs
    3 A-Levels
    BSc in the Internet of Things
```

That’s all you really need to know about YAML to start writing Ansible playbooks.

## Gotchas
While YAML is generally friendly, the following is going to result in a YAML syntax error:

```
foo: somebody said I should put a colon here: so I did
```

You will want to quote any hash values using colons, like so:

```yaml
foo: "somebody said I should put a colon here: so I did"
```

And then the colon will be preserved.

Further, Ansible uses “{{ var }}” for variables. If a value after a colon starts with a “{”, YAML will think it is a dictionary, so you must quote it, like so:

```yaml
foo: "{{ variable }}"
```

The same applies for strings that start or contain any YAML special characters `` [] {} : > | `` .

Boolean conversion is helpful, but this can be a problem when you want a literal yes or other boolean values as a string. In these cases just use quotes:

```yaml
non_boolean: "yes"
other_string: "False"
```