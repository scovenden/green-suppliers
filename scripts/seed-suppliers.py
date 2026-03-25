import requests, json, sys

API = "https://api.greensuppliers.co.za/api/v1"

# Login
login = requests.post(f"{API}/auth/login", json={"email": "admin@greensuppliers.co.za", "password": "ChangeMe123!"})
TOKEN = login.json()["data"]["accessToken"]
headers = {"Content-Type": "application/json", "Authorization": f"Bearer {TOKEN}"}

suppliers = [
    {
        "companyName": "EcoPack South Africa (Pty) Ltd",
        "tradingName": "EcoPack",
        "description": "EcoPack manufactures 100% biodegradable and compostable food packaging products from sugar cane, plant starches and paper board sourced from managed plantations. Their product range includes takeaway containers, cups, plates, bowls and cutlery designed to decompose naturally within 90 days.",
        "shortDescription": "Premier manufacturer of 100% biodegradable and compostable food packaging from sugar cane and plant starches.",
        "countryCode": "ZA", "city": "Cape Town", "province": "Western Cape",
        "website": "https://ecopack.co.za", "email": "info@ecopack.co.za",
        "renewableEnergyPercent": 30, "wasteRecyclingPercent": 85,
        "carbonReporting": True, "waterManagement": False, "sustainablePackaging": True,
        "industryIds": ["488a8f4d-337d-47e0-87e1-6a9f1cd9bbe3", "1cb7a02b-169e-48d1-885a-733cd5b14512"],
        "serviceTagIds": []
    },
    {
        "companyName": "Soventix South Africa (Pty) Ltd",
        "tradingName": "Soventix SA",
        "description": "Soventix South Africa is a solar PV EPC company focused on designing, procuring and constructing large Commercial and Industrial, as well as Utility-scale solar PV projects. The company ensures compliance with ISO 14001 and ISO 9001 standards.",
        "shortDescription": "Solar PV EPC company specialising in large-scale commercial, industrial and utility solar projects. ISO 14001 and 9001 certified.",
        "countryCode": "ZA", "city": "Johannesburg", "province": "Gauteng",
        "website": "https://www.soventix.com", "email": "info@soventix.co.za",
        "renewableEnergyPercent": 90, "wasteRecyclingPercent": 25,
        "carbonReporting": True, "waterManagement": False, "sustainablePackaging": False,
        "industryIds": ["95f21984-c437-4072-b076-eaf3f85609da"],
        "serviceTagIds": []
    },
    {
        "companyName": "Envirocrete (Pty) Ltd",
        "tradingName": "Envirocrete",
        "description": "Envirocrete specialises in green construction by manufacturing ecological housing solutions through innovative wood-concrete technology. Their Envirocrete Raw lightweight aggregate and unique green concrete mix promote sustainable building practices.",
        "shortDescription": "Green construction innovator manufacturing ecological housing solutions through wood-concrete technology.",
        "countryCode": "ZA", "city": "Pretoria", "province": "Gauteng",
        "website": "https://www.envirocrete.co.za",
        "renewableEnergyPercent": 20, "wasteRecyclingPercent": 70,
        "carbonReporting": True, "waterManagement": False, "sustainablePackaging": False,
        "industryIds": ["306cc47c-4052-4301-9aca-26f6b0dd0bd5", "488a8f4d-337d-47e0-87e1-6a9f1cd9bbe3"],
        "serviceTagIds": []
    },
    {
        "companyName": "Water Life Systems Africa (Pty) Ltd",
        "tradingName": "Water Life Systems",
        "description": "Water Life Systems Africa is a leading IoT and decentralized infrastructure technology integration company headquartered in Cape Town. The company focuses on smart water management solutions, water recycling systems and decentralized water treatment.",
        "shortDescription": "Leading IoT-enabled smart water management and decentralized water treatment solutions provider.",
        "countryCode": "ZA", "city": "Cape Town", "province": "Western Cape",
        "website": "https://www.waterlifesystems.co.za",
        "renewableEnergyPercent": 30, "wasteRecyclingPercent": 50,
        "carbonReporting": False, "waterManagement": True, "sustainablePackaging": False,
        "industryIds": ["e3fcdb3d-f742-407d-96f7-928c9d35d32b"],
        "serviceTagIds": []
    },
    {
        "companyName": "SOGA Organic (Pty) Ltd",
        "tradingName": "SOGA Organic",
        "description": "SOGA Organic is a certified organic citrus grower and exporter based in the Sundays River Valley, Eastern Cape. Their farms and packhouse are certified organic by Control Union and hold GlobalGAP and Natures Choice accreditation.",
        "shortDescription": "Certified organic citrus grower and exporter with Control Union, GlobalGAP and Natures Choice accreditation.",
        "countryCode": "ZA", "city": "Gqeberha", "province": "Eastern Cape",
        "website": "https://sogaorganic.co.za",
        "renewableEnergyPercent": 20, "wasteRecyclingPercent": 60,
        "carbonReporting": True, "waterManagement": True, "sustainablePackaging": True,
        "industryIds": ["7e655b9f-302d-4eb3-b1fa-a6f9b7400a99"],
        "serviceTagIds": []
    },
    {
        "companyName": "GreenCape NPC",
        "tradingName": "GreenCape",
        "description": "GreenCape is a non-profit organisation that drives the widespread adoption of economically viable green economy solutions in South Africa. They support businesses and investors through market intelligence, policy advocacy, and investment facilitation across renewable energy, waste, water and sustainable agriculture.",
        "shortDescription": "Non-profit driving green economy adoption in South Africa through market intelligence and investment facilitation.",
        "countryCode": "ZA", "city": "Cape Town", "province": "Western Cape",
        "website": "https://www.greencape.co.za", "email": "info@greencape.co.za",
        "renewableEnergyPercent": 50, "wasteRecyclingPercent": 40,
        "carbonReporting": True, "waterManagement": True, "sustainablePackaging": False,
        "industryIds": ["95f21984-c437-4072-b076-eaf3f85609da", "1cb7a02b-169e-48d1-885a-733cd5b14512", "e3fcdb3d-f742-407d-96f7-928c9d35d32b"],
        "serviceTagIds": []
    },
    {
        "companyName": "Detpak South Africa",
        "tradingName": "Detpak SA",
        "description": "Detpak designs, manufactures and supplies the foodservice, retail and industrial markets with world-class paper packaging products from its South African factory. As a member of FibreCircle, Detpak is legally compliant with the National Environmental Management Waste Act.",
        "shortDescription": "World-class paper packaging manufacturer for foodservice and retail, compliant with NEMWA waste regulations.",
        "countryCode": "ZA", "city": "Johannesburg", "province": "Gauteng",
        "website": "https://www.detpak.co.za",
        "renewableEnergyPercent": 20, "wasteRecyclingPercent": 75,
        "carbonReporting": True, "waterManagement": False, "sustainablePackaging": True,
        "industryIds": ["488a8f4d-337d-47e0-87e1-6a9f1cd9bbe3", "1cb7a02b-169e-48d1-885a-733cd5b14512"],
        "serviceTagIds": []
    },
    {
        "companyName": "Ecolution Consulting (Pty) Ltd",
        "tradingName": "Ecolution",
        "description": "Ecolution is a leading South African green building certification consultancy offering GBCSA Green Star, LEED, WELL and EDGE certification services. They help developers achieve sustainable building standards through energy modeling and materials assessment.",
        "shortDescription": "Green building certification consultancy specialising in Green Star, LEED, WELL and EDGE ratings.",
        "countryCode": "ZA", "city": "Johannesburg", "province": "Gauteng",
        "website": "https://ecolution.co.za", "email": "info@ecolution.co.za",
        "renewableEnergyPercent": 40, "wasteRecyclingPercent": 50,
        "carbonReporting": True, "waterManagement": True, "sustainablePackaging": False,
        "industryIds": ["306cc47c-4052-4301-9aca-26f6b0dd0bd5"],
        "serviceTagIds": []
    },
    {
        "companyName": "Polyco PRO NPC",
        "tradingName": "Polyco",
        "description": "Polyco is a non-profit company progressing the collection and recycling of plastic packaging across South Africa. Polyco promotes the responsible use and reuse of all plastic packaging with the aim of ending plastic waste in the environment.",
        "shortDescription": "Non-profit driving plastic packaging collection and recycling across South Africa for EPR compliance.",
        "countryCode": "ZA", "city": "Johannesburg", "province": "Gauteng",
        "website": "https://www.polyco.co.za", "email": "info@polyco.co.za",
        "renewableEnergyPercent": 15, "wasteRecyclingPercent": 95,
        "carbonReporting": True, "waterManagement": False, "sustainablePackaging": True,
        "industryIds": ["1cb7a02b-169e-48d1-885a-733cd5b14512"],
        "serviceTagIds": []
    },
    {
        "companyName": "Nature Pack (Pty) Ltd",
        "tradingName": "Nature Pack",
        "description": "Nature Pack is a South African manufacturer of eco-friendly packaging solutions including biodegradable food containers, compostable bags and sustainable retail packaging. The company sources raw materials from renewable resources.",
        "shortDescription": "Eco-friendly packaging manufacturer producing biodegradable food containers and compostable bags.",
        "countryCode": "ZA", "city": "Durban", "province": "KwaZulu-Natal",
        "website": "https://naturepack.co.za",
        "renewableEnergyPercent": 25, "wasteRecyclingPercent": 80,
        "carbonReporting": False, "waterManagement": False, "sustainablePackaging": True,
        "industryIds": ["488a8f4d-337d-47e0-87e1-6a9f1cd9bbe3"],
        "serviceTagIds": []
    },
    {
        "companyName": "Foster International Packaging",
        "tradingName": "Foster Packaging",
        "description": "Foster is one of the leading packaging suppliers in South Africa aiming to move away from single-use plastic and offering more earth-friendly, high-quality packaging options for the foodservice and retail industries.",
        "shortDescription": "Earth-friendly packaging supplier moving away from single-use plastic for foodservice and retail.",
        "countryCode": "ZA", "city": "Cape Town", "province": "Western Cape",
        "website": "https://fosterpackaging.com",
        "renewableEnergyPercent": 20, "wasteRecyclingPercent": 65,
        "carbonReporting": False, "waterManagement": False, "sustainablePackaging": True,
        "industryIds": ["488a8f4d-337d-47e0-87e1-6a9f1cd9bbe3"],
        "serviceTagIds": []
    }
]

success = 0
for i, s in enumerate(suppliers, 1):
    r = requests.post(f"{API}/admin/suppliers", json=s, headers=headers)
    try:
        data = r.json()
        ok = data.get("success", False)
        if ok:
            success += 1
            print(f"  OK: {s['tradingName']}")
        else:
            msg = data.get("error", {}).get("message", "Unknown error")
            print(f"  FAIL: {s['tradingName']} - {msg}")
    except:
        print(f"  ERROR: {s['tradingName']} - HTTP {r.status_code}")

print(f"\nSeeded {success}/{len(suppliers)} suppliers successfully")
