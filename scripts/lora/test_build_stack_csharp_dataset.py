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
