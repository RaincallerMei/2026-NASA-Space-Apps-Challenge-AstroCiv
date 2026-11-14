"""Document the shipped configuration without inventing or tuning game values."""
import csv
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
RESOURCE = ROOT / "Assets/_Game/Data/Resources"


def unit(name):
    if name.endswith("KW"):
        return "kW"
    if name.endswith("KWh"):
        return "kWh"
    if name.endswith("Lph"):
        return "L/game-hour"
    if name == "realSecondsPerGameHour":
        return "real seconds/game hour at 1x"
    if name.endswith("WaterL"):
        return "L"
    if "Meals" in name or name == "harvestMeals":
        return "meals"
    if name == "foodPerHour":
        return "meals/game hour"
    if name == "airBonusPerHour" or name == "airRefillPerHour" or name == "airDrainPerHour":
        return "air-hours/game hour"
    if name.endswith("PerHour"):
        return "safety points/game hour"
    if name.endswith("Hours") or name.endswith("Hour"):
        return "game hours"
    if name == "mealsPerCrewPerDay":
        return "meals/crew member/game day"
    if name.endswith("Fraction") or name in {"factor", "sunFloor"}:
        return "fraction"
    if "Percent" in name:
        return "percent"
    if name in {"cost", "startSupplies", "harvestSupplyReward"}:
        return "build points"
    if name in {"width", "height"}:
        return "tiles"
    if name in {"perfectSafety", "closeCallSafety", "safetyFailThreshold"}:
        return "safety points (0-100)"
    if name == "priority":
        return "priority rank (lower is more critical)"
    return "count"


def main():
    balance = json.loads((RESOURCE / "Balance.json").read_text(encoding="utf-8"))
    rows = []

    def add(variable, value, units, source, classification, confidence, use):
        rows.append([variable, units, value, source, classification, confidence, use])

    def flatten(data, prefix=""):
        for name, value in data.items():
            path = prefix + name
            if isinstance(value, dict):
                flatten(value, path + ".")
            elif isinstance(value, list):
                if name == "buildings":
                    for building in value:
                        flatten({k: v for k, v in building.items() if k != "type"}, "buildings." + building["type"] + ".")
                elif name == "rockTiles":
                    add(path, json.dumps(value), "tile x,y pairs", "Balance.json: " + path,
                        "fictional balance", "Exact shipped setting; not a measured terrain product", "Blocked construction tiles")
            elif isinstance(value, (int, float)) and not isinstance(value, bool):
                add(path, value, unit(name), "Balance.json: " + path, "fictional balance",
                    "Exact shipped setting; not validated as real equipment", "Existing simulation")

    flatten(balance)
    greenhouse = next(d for d in balance["buildings"] if d["type"] == "Greenhouse")
    add("greenhouse.recycledFraction", greenhouse["waterReturnLph"] / greenhouse["waterInLph"], "fraction",
        "Balance.json: Greenhouse waterReturnLph / waterInLph", "fictional balance", "Derived exactly from configuration", "Water recycling")
    add("crewMealsPerHour", balance["crewCount"] * balance["mealsPerCrewPerDay"] / 24, "meals/game hour",
        "Balance.json: crewCount * mealsPerCrewPerDay / 24", "fictional balance", "Derived exactly from configuration", "Food consumption")

    power = json.loads((RESOURCE / "Fixtures/power_mississauga.json").read_text(encoding="utf-8"))
    add("mississauga.annualSunlight", power["annual"], power["units"], power["query_url"], power["classification"],
        "Saved NASA response; see retrieval date in fixture", "Existing local-impact card; Earth surface only")
    for variable, name in [("storm.eventMetadata", "donki_event_pack"), ("biology.studyCards", "osdr_cards"), ("greenhouse.scienceCards", "csa_food_cards")]:
        fixture = json.loads((RESOURCE / ("Fixtures/" + name + ".json")).read_text(encoding="utf-8"))
        add(variable, "See saved fixture", fixture["units"], fixture["query_url"], fixture["classification"],
            "Source-derived reference; not simulation performance data", "Existing card / source display")
    for name, use in [("radlab_iss_sample", "ISS reference card; not lunar dose"), ("illumination_grid", "20x15 tile sunlight; not live shadows")]:
        header = {}
        for line in (RESOURCE / ("Fixtures/" + name + ".csv")).read_text(encoding="utf-8").splitlines():
            if line.startswith("# ") and ":" in line:
                key, value = line[2:].split(":", 1)
                header[key] = value.strip()
        add(name, "See saved fixture", header["units"], header["query_url"], header["classification"],
            "Saved source-derived sample; see DATA_SOURCES.md", use)

    target = ROOT / "SCIENCE_ASSUMPTIONS.csv"
    with target.open("w", encoding="utf-8", newline="") as output:
        writer = csv.writer(output)
        writer.writerow(["variable", "unit", "value_or_range", "source", "classification", "confidence", "gameplay_use"])
        writer.writerows(rows)
    print(f"Wrote {len(rows)} existing settings/reference rows to {target.name}.")


if __name__ == "__main__":
    main()
