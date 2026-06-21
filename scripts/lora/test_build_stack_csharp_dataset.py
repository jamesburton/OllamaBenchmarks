import build_stack_csharp_dataset as b


def test_passes_license_accepts_mit():
    assert b.passes_license({"license": "MIT"}) is True


def test_passes_license_accepts_list_field():
    assert b.passes_license({"max_stars_repo_licenses": ["Apache-2.0"]}) is True


def test_passes_license_rejects_gpl():
    assert b.passes_license({"license": "GPL-3.0"}) is False


def test_passes_license_rejects_missing():
    assert b.passes_license({}) is False


def test_passes_size_within_bounds():
    assert b.passes_size("x" * 500) is True


def test_passes_size_too_small():
    assert b.passes_size("tiny") is False


def test_passes_size_too_large():
    assert b.passes_size("x" * 9000) is False


def test_is_modern_csharp_true_for_record():
    assert b.is_modern_csharp("public record Person(string Name);") is True


def test_is_modern_csharp_true_for_async_namespace():
    code = "namespace App;\npublic class C { async Task M() {} }"
    assert b.is_modern_csharp(code) is True


def test_is_modern_csharp_false_for_legacy():
    assert b.is_modern_csharp("class Foo { void Bar() {} }") is False


def test_extract_functions_finds_one():
    code = (
        "public int Add(int a, int b)\n"
        "{\n"
        "    return a + b;\n"
        "}\n"
    )
    fns = b.extract_functions(code)
    assert len(fns) == 1
    sig, body = fns[0]
    assert "Add(int a, int b)" in sig
    assert "return a + b;" in body
    assert body.strip().startswith("{")
    assert body.strip().endswith("}")


def test_extract_functions_handles_nested_braces():
    code = (
        "public void M()\n"
        "{\n"
        "    if (true) { Console.WriteLine(1); }\n"
        "}\n"
    )
    fns = b.extract_functions(code)
    assert len(fns) == 1
    assert "Console.WriteLine(1);" in fns[0][1]


def test_extract_functions_empty_when_none():
    assert b.extract_functions("int x = 5;") == []


def test_to_chat_example_shape():
    ex = b.to_chat_example("public int Add(int a, int b)", "{ return a + b; }")
    roles = [m["role"] for m in ex["messages"]]
    assert roles == ["system", "user", "assistant"]
    assert ex["messages"][0]["content"] == b.SYSTEM_PROMPT
    assert "Add(int a, int b)" in ex["messages"][1]["content"]
    assert ex["messages"][2]["content"].startswith("public int Add")
    assert "return a + b;" in ex["messages"][2]["content"]


# --- Defect 1: brace matching must not stop at braces inside string literals ---

def test_extract_functions_body_with_brace_in_string():
    """A `}` inside a double-quoted string must not prematurely close the block."""
    code = (
        'public string Foo()\n'
        '{\n'
        '    var s = "}";\n'
        '    return s;\n'
        '}\n'
    )
    fns = b.extract_functions(code)
    assert len(fns) == 1
    sig, body = fns[0]
    body_stripped = body.strip()
    assert body_stripped.endswith("}")
    assert 'return s;' in body
    assert '"}"' in body


def test_extract_functions_verbatim_string_brace():
    """A `}` inside a verbatim string `@\"...\"` must not prematurely close the block."""
    code = (
        'public string Bar()\n'
        '{\n'
        '    var s = @"a}b";\n'
        '    return s;\n'
        '}\n'
    )
    fns = b.extract_functions(code)
    assert len(fns) == 1
    sig, body = fns[0]
    body_stripped = body.strip()
    assert body_stripped.endswith("}")
    assert 'a}b' in body


# --- Defect 2: constructors must not be extracted as methods ---

def test_extract_functions_ignores_constructor():
    """Constructor signatures must be skipped — no return type means no valid method."""
    code = (
        'public Widget(int x)\n'
        '{\n'
        '    X = x;\n'
        '}\n'
    )
    fns = b.extract_functions(code)
    assert fns == []


def test_extract_functions_keeps_method_returning_pascalcase():
    """A method whose return type is a PascalCase class name must NOT be dropped."""
    code = (
        'public Person GetPerson(int id)\n'
        '{\n'
        '    return null;\n'
        '}\n'
    )
    fns = b.extract_functions(code)
    assert len(fns) == 1
    assert 'GetPerson' in fns[0][0]


def _ex(content):
    return {"messages": [
        {"role": "system", "content": b.SYSTEM_PROMPT},
        {"role": "user", "content": "u"},
        {"role": "assistant", "content": content},
    ]}


def test_content_key_ignores_whitespace():
    assert b.content_key(_ex("a  b\n c")) == b.content_key(_ex("a b c"))


def test_dedup_removes_duplicates():
    items = [_ex("same"), _ex("same"), _ex("different")]
    out = b.dedup(items)
    assert len(out) == 2


def test_split_holdout_sizes():
    items = [_ex(str(i)) for i in range(100)]
    train, holdout = b.split_holdout(items, fraction=0.10, seed=42)
    assert len(holdout) == 10
    assert len(train) == 90
    # disjoint
    train_keys = {b.content_key(x) for x in train}
    hold_keys = {b.content_key(x) for x in holdout}
    assert train_keys.isdisjoint(hold_keys)


def test_split_holdout_deterministic():
    items = [_ex(str(i)) for i in range(100)]
    a1, b1 = b.split_holdout(items, seed=42)
    a2, b2 = b.split_holdout(items, seed=42)
    assert [b.content_key(x) for x in b1] == [b.content_key(x) for x in b2]
