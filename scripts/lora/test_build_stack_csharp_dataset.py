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
