/**
* @file MatchTemplate_Demo.cpp
* @brief Sample code to use the function MatchTemplate
* @author OpenCV team
*/

#include "stdafx.h"

using namespace std;
using namespace cv;
Mat windowToMat(LPTSTR name);
//! [declare]

const char* image_windowName = "Source Image";
const char* result_windowName = "Result window";

/// Function Headers
void MatchingMethod1(Mat orig , Mat diff);


Mat getGrayScale(Mat color) {	
	cv::Mat gs_rgb(color.size(), CV_8UC1);
	cv::cvtColor(color, gs_rgb, CV_RGB2GRAY);
	return gs_rgb;
}
/**
* @function main
*/


std::vector<Mat> breakImagesHor(Mat original, int PAD=2) {
	int xstart = -1;
	int topy = -1;
	int bottomy = original.rows;
	std::vector<Mat> imgs;
	for (int x = 0; x < original.cols; x++) {
		if (xstart == -1) {
			for (int j = 0; j < original.rows; ++j)
			{
				if (original.ptr<uchar>(j)[x] > 0) {
					xstart = x;
					if (topy < 0) topy = j;
					bottomy = j;
				}
			}
		}
		
		
		if (xstart >= 0){
			bool ended = true;
			for (int j = 0; j < original.rows; ++j)
			{
				if (original.ptr<uchar>(j)[x] > 0) {
					if (topy > j) topy = j;
					if (bottomy < j) bottomy = j;
					ended = false;
				}
			}
			if (ended) {
				int besth = bottomy - topy + 1;
				int iw = x - xstart;
				if (besth > 1 && iw > 1) {
					Mat digit(besth+ PAD, iw+ PAD, original.type(), Scalar(0));
					original(Rect(xstart, topy, iw, besth)).copyTo(digit(Rect(1,1,iw,besth)));
					imgs.push_back(digit);
				}
				xstart = -1;
				bottomy = original.rows;
				topy = -1;
			}
		}
	}

	return imgs;
}

struct BlockInfo {
public :	
	Rect rect;
	char * info;
	int Threadshold;
	BlockInfo(Rect r, int threadshold = 230, char* inf= NULL) {
		rect = r;
		info = inf;
		Threadshold = threadshold;
	}
};

Mat loadImageRect(Mat img, BlockInfo blk) {
	Rect rect = blk.rect;
	if (rect.width <= 0) {
		rect.width = img.cols - rect.x;
	}
	Mat numBlock;
	numBlock.create(rect.height, rect.width, img.type());
	for (int j = 0; j < numBlock.rows; ++j)
	{
		const uchar* current = img.ptr<uchar>(j + rect.y);

		uchar* output = numBlock.ptr<uchar>(j);

		for (int i = 0; i < (numBlock.cols); i++)
		{
			uchar cval = current[i + rect.x];
			*output = cval;
			*output = 0;
			if (cval > blk.Threadshold) {
				*output = 255;
			}
			output++;
		}
	}

	return numBlock;
}
std::vector<Mat> doTopNumbers(Mat img, BlockInfo blk, int PAD=2) {
	
	//Mat img = getGrayScale(imread("data\\cctxt\\archerl1.JPG", IMREAD_COLOR));
	Mat numBlock =loadImageRect(img, blk);
	

	char buf[512];
	sprintf_s(buf, "tstimgs\\test%s_%i.png", blk.info, -1);
	imwrite(buf, numBlock);
	std::vector<Mat> digits = breakImagesHor(numBlock, PAD);
	for (int i = 0; i < digits.size(); i++) {		
		sprintf_s(buf, "tstimgs\\test%s_%i.png", blk.info, i);
		imwrite(buf, digits[i]);
	}
	return digits;
}

struct RecoInfo{
public:
	Mat img;
	char chr;
	RecoInfo(Mat i, char ch) {
		img = i;
		chr = ch;
	}
};

struct RecoList {
public:
	vector<RecoInfo> recoInfo;
};

int StrEndsWith(char *str, char * ending)
{
	if (strlen(str) >= strlen(ending))
	{
		if (!strcmp(str + strlen(str) - strlen(ending), ending))
		{
			return 1;
		}
	}
	return 0;
}

RecoList LoadDataInfo(const char * dir) {
	DIR *d = opendir(dir);
	dirent * de;
	RecoList res;
	while (de = readdir(d)) {
		if (StrEndsWith(de->d_name, ".png")) {
			printf("%s %c\n", de->d_name, de->d_name[2]);
			char buf[512];
			sprintf_s(buf, "%s\\%s", dir, de->d_name);
			Mat img = getGrayScale(imread(buf));
			res.recoInfo.push_back(RecoInfo(img, de->d_name[2]));
		}
	}
	closedir(d);
	return res;
}


struct ImageDiffVal {
public:
	int x;
	int y;
	float val;
	ImageDiffVal(int xx, int yy, float v) {
		x = xx;
		y = yy;
		val = v;
	}
	bool operator<(const ImageDiffVal &rhs) const { return val < rhs.val; }
};

vector<ImageDiffVal> GetTopXGoodones(Mat result, int max) {
	vector<ImageDiffVal> vals;
	for (int y = 0; y < result.rows; y++) {
		float* current = result.ptr<float>(y);
		for (int x = 0; x < result.cols; x++) {
			float pt = current[x];
			vals.push_back(ImageDiffVal(x, y, pt));
			sort(vals.begin(), vals.end());
			if (vals.size() > max) {
				vals.pop_back();
			}
		}
	}
	return vals;
}

struct ImageRecoRes {
public:
	static int xspacing;
	int x;
	int trimedX;
	char c;
	float val;
	ImageRecoRes(int xx, char chr, float v) {
		x = xx;
		trimedX = xx / xspacing;
		c = chr;
		val = v;
	}
	bool operator<(const ImageRecoRes &rhs) const { return val < rhs.val; }
};


bool SortImageRecoResByX(ImageRecoRes i, ImageRecoRes j) { return (i.x<j.x); }
int ImageRecoRes::xspacing = 10;
int gmatch_method = CV_TM_SQDIFF;

struct ImageFindLoc {
public:
	Point loc;
	double val;
	bool found = false;
	ImageFindLoc(Point p, double v, bool good) {
		loc = p;
		val = v;
		found = good;
	}
};
ImageFindLoc CheckAttackedDialog(Mat img) {
	Mat result;
	Mat templ = getGrayScale(imread("data\\check\\ememyattacked.png", IMREAD_COLOR));
	Mat mask = (imread("data\\check\\ememyattacked.mask.bmp", IMREAD_GRAYSCALE));
	matchTemplate(img, templ, result, gmatch_method, mask);
	double minVal; double maxVal; Point minLoc; Point maxLoc;
	Point matchLoc;
	minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
	//printf("attaced dialog find min at %i %i val %f\n", minLoc.x, minLoc.y, minVal);
	return ImageFindLoc(minLoc, minVal, minVal < 200000);
}



ImageFindLoc CheckImageMatch(Mat img, char * fileName) {
	Mat result;
	Mat templ = getGrayScale(imread(fileName, IMREAD_COLOR));
	matchTemplate(img, templ, result, gmatch_method);
	double minVal; double maxVal; Point minLoc; Point maxLoc;
	Point matchLoc;
	minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
	//printf("find min at %i %i val %f\n", minLoc.x, minLoc.y, minVal);
	return ImageFindLoc(minLoc, minVal, minVal < 200000);
}

ImageFindLoc CheckDialogLoadVilege(Mat img) {
	return CheckImageMatch(img, "data\\check\\loadVillage.png");
}

ImageFindLoc CheckDialogConfirmLoadVilege(Mat img) {
	return CheckImageMatch(img, "data\\check\\confirm.bmp");
}

//return true if a is better
bool checkIfFirstRecoBetter(float a, float b) {
	return a < b;
}
bool debug = false;
vector<ImageRecoRes> DoReco(RecoList list, Mat img, int blkNumber) {
	
	vector<ImageRecoRes> res;
	if (debug && blkNumber != 1) return res;
	
	float VALMAX = 6551750;
	for (int i = 0; i < list.recoInfo.size(); i++) {
		
		Mat result;
		RecoInfo recInfo = list.recoInfo[i];
		if (debug && (recInfo.chr != 'l' && recInfo.chr != 'E')) continue;
		Mat templ = recInfo.img;
		printf("checking %c at blk %i\n", recInfo.chr, blkNumber);
		int result_cols = img.cols - templ.cols + 1;
		int result_rows = img.rows - templ.rows + 1;

		if (result_cols <= 0 || result_rows <= 0) {
			printf("image not matching %i %i\n", result_cols, result_rows);
			continue;
		}
		
		result.create(result_rows, result_cols, CV_32FC1);
		matchTemplate(img, templ, result, gmatch_method);
		
		//normalize(result, result, 1,0, NORM_MINMAX, -1, Mat());
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		Point matchLoc;
		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());

		
		//minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());		

		//int steps[10];		
		//for (int y = 0; y < 10; y++) steps[y] = 0;
		//for (int y = 0; y < result.rows; y++) {
		//	float* current = result.ptr<float>(y);
		//	for (int x = 0; x < result.cols; x++) {
		//		int pt = (current[x] * 10);
		//		if (pt >= 10) pt = 9;
		//		if (pt < 0) {
		//			pt = 0; 
		//		}
		//		if (pt >1) current[x] = 255;
		//		//else printf("found low at %i %i %f\n", x, y, current[x]);
		//		steps[pt]++;
		//	}
		//}
		//for (int y = 0; y <5; y++) {
		//	printf("at %i has %i\n", y, steps[y]);
		//}
		char buf[512];
		//sprintf_s(buf, "tstimgs\\blk%i_check%c_%i.png", blkNumber, recInfo.chr, i);
		//imwrite(buf, result);
		sprintf_s(buf, "tstimgs\\blk%i.png", blkNumber);
		imwrite(buf, img);
		vector<ImageDiffVal> topvals = GetTopXGoodones(result, 5);

		for (int y = 0; y < topvals.size(); y++) {
			ImageDiffVal cur = topvals[y];
			if (cur.val > VALMAX) continue;
			//int xspace = cur.x / ImageRecoRes::xspacing;
			ImageRecoRes xspace = ImageRecoRes(cur.x, recInfo.chr, cur.val);

			bool found = false;
			for (vector<ImageRecoRes>::iterator it = res.begin(); it != res.end(); it++) {
				ImageRecoRes existing = *it;
				//printf("xrrtop %c at %i %i has %f t%i xp%i\n", recInfo.chr, cur.x, cur.y, cur.val, existing.trimedX, xspace.trimedX);
				if (existing.trimedX == xspace.trimedX) {
					found = true;
					if (checkIfFirstRecoBetter(cur.val, existing.val) )
					{						
						it->val = cur.val;	
						it->c = recInfo.chr;
						printf("rrtop %c at %i %i has %f\n", recInfo.chr, cur.x, cur.y, cur.val);
					}
					break;
				}
			}
			if (!found) {
				res.push_back(xspace);
				printf("top %c at %i %i has %f\n", recInfo.chr, cur.x, cur.y, cur.val);
			}			
		}
		//namedWindow(result_windowName, WINDOW_NORMAL);
		//namedWindow(image_windowName, WINDOW_NORMAL);
		//imshow(image_windowName, img);
		//imshow(result_windowName, result);
		//break;
	}

	//sort(res.begin(), res.end(), SortImageRecoResByX);
	sort(res.begin(), res.end());
	vector<ImageRecoRes> ret;
	for (int i = 0; i < res.size(); i++) {
		ImageRecoRes cur = res[i];
		if (i > 0) {			
			ImageRecoRes prev = res[i - 1];
			if (cur.x  - prev.x < ImageRecoRes::xspacing) {
				if (checkIfFirstRecoBetter(cur.val, prev.val)) {
					res[i - 1] = cur;
				}
			}
			else {
				ret.push_back(cur);
			}
		}
		else ret.push_back(cur);
		break;
	}
	return ret;
}

Mat LoadCCScreen() {
	return windowToMat(L"cctest [Running] - Oracle VM VirtualBox");
}

void printCheckLocation(ImageFindLoc where, const char * who, Point move) {
	if (where.found) {
		printf("%s %i %i %f\n", who, where.loc.x + move.x, where.loc.y + move.y, where.val);
	}
}
Mat doChecks() {
	Mat img = getGrayScale(LoadCCScreen());
	printCheckLocation(CheckAttackedDialog(img), "VillageAttacked", Point(345,440));	
	printCheckLocation(CheckDialogLoadVilege(img), "LoadingVillage", Point(298,44));
	printCheckLocation(CheckDialogConfirmLoadVilege(img), "ConfirmLoadVillage", Point(310,22));
	return img;
}
int main(int argc, char** argv)
{	
	for (int i = 1; i < argc; i++) {
		if (strcmp(argv[i], "-check") == 0) {
			doChecks();
			return 0;
		}
	}
	int PAD = 2;
	Mat screen = LoadCCScreen();
	imwrite("tstimgs\\full.png", screen);
	RecoList checkList = LoadDataInfo("data\\check\\bottom");
	int thd = 220;
	Mat img = getGrayScale(imread("tstimgs\\full.png", IMREAD_COLOR));
	doTopNumbers(img, BlockInfo(Rect(780,  42,-1, 30), thd, "gld"));
	doTopNumbers(img, BlockInfo(Rect(780, 105,-1, 30), thd,"elis"));
	vector<Mat> blocks = doTopNumbers(img, BlockInfo(Rect(280, 605, -1,45 + PAD), thd, "bottom"), 5);

	//Mat img = getGrayScale(imread("data\\cctxt\\archerl1.JPG", IMREAD_COLOR));	
	//Mat numBlock = loadImageRect(img, BlockInfo(Rect(280, 605, -1, 45)));
	vector<ImageRecoRes> res;
	for (int imgblk = 0; imgblk < blocks.size(); imgblk++) {
		printf("doing at next img\n");
		vector<ImageRecoRes> stepres = DoReco(checkList, blocks[imgblk], imgblk);
		res.insert(res.end(), stepres.begin(), stepres.end());
		printf("Reco Result:");
		for (vector<ImageRecoRes>::iterator it = res.begin(); it != res.end(); it++) {
			printf("%c", it->c);
		}
		printf("\n");
	}
	
	doChecks();
	waitKey(0);
	return 0;
	//! [load_image]
	/// Load image and template
	Mat dOrigImg = getGrayScale(imread("data\\cctxt\\lib1.jpg", IMREAD_COLOR));
	Mat diffImage = getGrayScale(imread("data\\cctxt\\no.jpg", IMREAD_COLOR));

	if (dOrigImg.empty() || diffImage.empty() )
	{
		cout << "Can't read one of the images" << endl;
		return -1;
	}

	Mat diffresult;
	cv::absdiff(dOrigImg, diffImage, diffresult);

	Mat result;
	result.create(diffresult.size(), diffresult.type());
	for (int j = 0; j < diffImage.rows; ++j)
	{
		const uchar* current = diffImage.ptr<uchar>(j);

		uchar* output = result.ptr<uchar>(j);

		for (int i = 0; i < (diffImage.cols); i++)
		{
			*output = 0;
			if (current[i] > 200) {
				*output = 255;
			}
			output++;
		}
	}

	//! [load_image]

	//! [create_windows]
	/// Create windows
	namedWindow(image_windowName, WINDOW_NORMAL);
	namedWindow(result_windowName, WINDOW_NORMAL);

	imshow(image_windowName, diffImage);
	imshow(result_windowName, result);

	waitKey(0);
	return 0;
	//! [create_windows]

	//! [create_trackbar]
	/// Create Trackbar
	const char* trackbar_label = "Method: \n 0: SQDIFF \n 1: SQDIFF NORMED \n 2: TM CCORR \n 3: TM CCORR NORMED \n 4: TM COEFF \n 5: TM COEFF NORMED";
	
	//! [create_trackbar]

	MatchingMethod1(dOrigImg, diffImage);

	//! [wait_key]
	waitKey(0);
	return 0;
	//! [wait_key]
}


/**
* @function MatchingMethod
* @brief Trackbar callback
*/
void MatchingMethod1(Mat img, Mat templ)
{
	Mat img_display;
	img.copyTo(img_display);

	Mat result;
	double oldVal;
	Mat firstResult;
	int result_cols = img.cols - templ.cols + 1;
	int result_rows = img.rows - templ.rows + 1;

	result.create(result_rows, result_cols, CV_32FC1);
	//for (int loopi = 0; loopi < 4; loopi++) 
	{
		
			int match_method = CV_TM_SQDIFF;
			matchTemplate(img, templ, result, match_method);
		

		/// Localizing the best match with minMaxLoc
		double minVal; double maxVal; Point minLoc; Point maxLoc;
		Point matchLoc;

		minMaxLoc(result, &minVal, &maxVal, &minLoc, &maxLoc, Mat());
		//! [best_match]

		normalize(result, result, 0, 1, NORM_MINMAX, -1, Mat());
		
		//! [match_loc]
		/// For SQDIFF and SQDIFF_NORMED, the best matches are lower values. For all the other methods, the higher the better
		if (match_method == TM_SQDIFF || match_method == TM_SQDIFF_NORMED)
		{
			oldVal = minVal;
			matchLoc = minLoc;
		}
		else
		{
			oldVal = maxVal;
			matchLoc = maxLoc;
		}

		printf("val is %f\n", (float)oldVal);
		//! [match_loc]

		//! [imshow]
		/// Show me what you got
		rectangle(img_display, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0), 2, 8, 0);
		rectangle(result, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0), 2, 8, 0);

		imshow(image_windowName, img_display);
		imshow(result_windowName, result);

		//rectangle(img, matchLoc, Point(matchLoc.x + templ.cols, matchLoc.y + templ.rows), Scalar::all(0),-1);
		//! [imshow]
	}
	return;
}